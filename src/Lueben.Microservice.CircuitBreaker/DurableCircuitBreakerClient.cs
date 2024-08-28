using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Caching;
using Polly.Registry;

namespace Lueben.Microservice.CircuitBreaker
{
    public partial class DurableCircuitBreakerClient : IDurableCircuitBreakerClient
    {
        private const string IsExecutionPermittedInternalOrchestratorName = "IsExecutionPermittedInternalOrchestratorName";
        private const string DurableCircuitBreakerKeyPrefix = "DurableCircuitBreaker-";

        private readonly IPolicyRegistry<string> _policyRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly OptionsManager<CircuitBreakerSettings> _options;

        public DurableCircuitBreakerClient(OptionsManager<CircuitBreakerSettings> options, IPolicyRegistry<string> policyRegistry, IServiceProvider serviceProvider)
            : this(policyRegistry, serviceProvider)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<bool> IsExecutionPermitted(string circuitBreakerId, ILogger log, IDurableOrchestrationContext orchestrationContext)
        {
            if (string.IsNullOrEmpty(circuitBreakerId))
            {
                throw new ArgumentNullException($"{nameof(circuitBreakerId)}");
            }

            log?.LogCircuitBreakerMessage(circuitBreakerId, $"Asking IsExecutionPermitted (consistency priority) for circuit-breaker = '{circuitBreakerId}'.");

            return await orchestrationContext.CreateEntityProxy<IDurableCircuitBreaker>(circuitBreakerId).IsExecutionPermitted();
        }

        public async Task<bool> IsExecutionPermitted_StrongConsistency(string circuitBreakerId, ILogger log, IDurableClient orchestrationClient, IConfiguration configuration)
        {
            // The circuit-breaker can be configured with a maximum time you are prepared to wait to obtain the current circuit state; this allows you to limit the circuit-breaker itself introducing unwanted excessive latency.
            var checkCircuitConfiguration = GetCheckCircuitConfiguration(circuitBreakerId);

            string executionPermittedInstanceId = await orchestrationClient.StartNewAsync(IsExecutionPermittedInternalOrchestratorName, circuitBreakerId);

            // We have to choose a course of action for when the circuit-breaker entity does not report state in a timely manner.
            // We choose to gracefully drop the circuit-breaker functionality and permit the execution (rather than a more aggressive option of, say, failing the execution).
            const bool permitExecutionOnCircuitStateQueryFailure = true;

            (bool? isExecutionPermitted, OrchestrationRuntimeStatus status) = await WaitForCompletionOrTimeout(executionPermittedInstanceId, checkCircuitConfiguration, orchestrationClient);

            switch (status)
            {
                case OrchestrationRuntimeStatus.Completed:
                    log?.LogCircuitBreakerMessage(circuitBreakerId, $"IsExecutionPermitted (consistency priority) for circuit-breaker = '{circuitBreakerId}' returned: {isExecutionPermitted}.");
                    return isExecutionPermitted.Value;
                default:
                    OnCircuitStateQueryFailure(status.ToString(), circuitBreakerId, log);
                    return permitExecutionOnCircuitStateQueryFailure;
            }
        }

        public async Task RecordSuccess(string circuitBreakerId, ILogger log, IDurableOrchestrationContext orchestrationContext)
        {
            log?.LogCircuitBreakerMessage(circuitBreakerId, $"Recording success for circuit-breaker = '{circuitBreakerId}'.");

            await orchestrationContext.CreateEntityProxy<IDurableCircuitBreaker>(circuitBreakerId).RecordSuccess();
        }

        public async Task RecordFailure(string circuitBreakerId, ILogger log, IDurableOrchestrationContext orchestrationContext)
        {
            log?.LogCircuitBreakerMessage(circuitBreakerId, $"Recording failure for circuit-breaker = '{circuitBreakerId}'.");

            await orchestrationContext.CreateEntityProxy<IDurableCircuitBreaker>(circuitBreakerId).RecordFailure();
        }

        public async Task<CircuitState> GetCircuitState(string circuitBreakerId, ILogger log, IDurableOrchestrationContext orchestrationContext)
        {
            log?.LogCircuitBreakerMessage(circuitBreakerId, $"Getting circuit state for circuit-breaker = '{circuitBreakerId}'.");

            return await orchestrationContext.CreateEntityProxy<IDurableCircuitBreaker>(circuitBreakerId).GetCircuitState();
        }

        public async Task<DurableCircuitBreaker> GetBreakerState(string circuitBreakerId, ILogger log, IDurableOrchestrationContext orchestrationContext)
        {
            log?.LogCircuitBreakerMessage(circuitBreakerId, $"Getting breaker state for circuit-breaker = '{circuitBreakerId}'.");

            return await orchestrationContext.CreateEntityProxy<IDurableCircuitBreaker>(circuitBreakerId).GetBreakerState();
        }

        private async Task<(bool? IsExecutionPermitted, OrchestrationRuntimeStatus Status)> WaitForCompletionOrTimeout(string executionPermittedInstanceId, (TimeSpan Timeout, TimeSpan RetryInterval) checkCircuitConfiguration, IDurableOrchestrationClient orchestrationClient)
        {
            var stopwatch = Stopwatch.StartNew();
            while (true)
            {
                var status = await orchestrationClient.GetStatusAsync(executionPermittedInstanceId);
                if (status != null)
                {
                    if (status.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                    {
                        try
                        {
                            return (status.Output.ToObject<bool>(), OrchestrationRuntimeStatus.Completed);
                        }
                        catch
                        {
                            return (null, OrchestrationRuntimeStatus.Unknown);
                        }
                    }

                    if (status.RuntimeStatus == OrchestrationRuntimeStatus.Canceled ||
                        status.RuntimeStatus == OrchestrationRuntimeStatus.Failed ||
                        status.RuntimeStatus == OrchestrationRuntimeStatus.Terminated)
                    {
                        return (null, status.RuntimeStatus);
                    }
                }

                var elapsed = stopwatch.Elapsed;
                if (elapsed >= checkCircuitConfiguration.Timeout)
                {
                    // Timed out.
                    return (null, OrchestrationRuntimeStatus.Pending);
                }

                var remainingTime = checkCircuitConfiguration.Timeout.Subtract(elapsed);

                await Task.Delay(remainingTime <= checkCircuitConfiguration.RetryInterval
                    ? remainingTime
                    : checkCircuitConfiguration.RetryInterval);
            }
        }

        private void OnCircuitStateQueryFailure(string failure, string circuitBreakerId, ILogger log)
        {
            // We log any circuit state query failure.
            // Production apps could of course alert (directly here or indirectly by configured alerts) on the circuit-breaker not responding in a timely manner; or choose other options.
            log?.LogCircuitBreakerMessage(circuitBreakerId, $"IsExecutionPermitted (consistency priority) for circuit-breaker = '{circuitBreakerId}': {failure}.");
        }

        private (TimeSpan Timeout, TimeSpan RetryInterval) GetCheckCircuitConfiguration(string circuitBreakerId)
        {
            var settings = _options.Get(circuitBreakerId);
            if (settings.ConsistencyPriorityCheckCircuitRetryIntervalTime > settings.ConsistencyPriorityCheckCircuitTimeoutTime)
            {
                throw new ArgumentException($"Total timeout {settings.ConsistencyPriorityCheckCircuitTimeoutTime.TotalSeconds} should be bigger than retry timeout {settings.ConsistencyPriorityCheckCircuitRetryIntervalTime.TotalSeconds}");
            }

            return (settings.ConsistencyPriorityCheckCircuitTimeoutTime, settings.ConsistencyPriorityCheckCircuitRetryIntervalTime);
        }

        public DurableCircuitBreakerClient(
            IPolicyRegistry<string> policyRegistry,
            IServiceProvider serviceProvider)
        {
            this._policyRegistry = policyRegistry;
            this._serviceProvider = serviceProvider;
        }

        public async Task<bool> IsExecutionPermitted(string circuitBreakerId, ILogger log, IDurableClient durableClient, IConfiguration configuration)
        {
            // The performance priority approach reads the circuit-breaker entity state from outside.
            // Per Azure Entity Functions documentation, this may be stale if other operations on the entity have been queued but not yet actioned,
            // but it returns faster than actually executing an operation on the entity (which would queue as a serialized operation against others).
            // The trade-off is that a true half-open state (permitting only one execution per breakDuration) cannot be maintained.
            log?.LogCircuitBreakerMessage(circuitBreakerId, $"Asking IsExecutionPermitted (performance priority) for circuit-breaker = '{circuitBreakerId}'.");

            var breakerState = await GetBreakerStateWithCaching(circuitBreakerId, () => GetBreakerState(circuitBreakerId, log, durableClient), configuration);

            bool isExecutionPermitted;
            if (breakerState == null)
            {
                // We permit execution if the breaker is not yet initialized; a not-yet-initialized breaker is deemed closed, for simplicity.
                // It will be initialized when the first success or failure is recorded against it.
                isExecutionPermitted = true;
            }
            else if (breakerState.CircuitState == CircuitState.HalfOpen || breakerState.CircuitState == CircuitState.Open)
            {
                // If the circuit is open or half-open, we permit executions if the broken-until period has passed.
                // Unlike the Consistency mode, we cannot control (since we only read state, not update it) how many executions are permitted in this state.
                // However, the first execution to fail in half-open state will push out the BrokenUntil time by BreakDuration, blocking executions until the next BreakDuration has passed.
                // (Or a success first will close the circuit again.)
                isExecutionPermitted = DateTime.UtcNow > breakerState.BrokenUntil;
            }
            else if (breakerState.CircuitState == CircuitState.Closed)
            {
                isExecutionPermitted = true;
            }
            else
            {
                throw new InvalidOperationException();
            }

            log?.LogCircuitBreakerMessage(circuitBreakerId, $"IsExecutionPermitted (performance priority) for circuit-breaker = '{circuitBreakerId}' returned: {isExecutionPermitted}.");
            return isExecutionPermitted;
        }

        public async Task RecordSuccess(string circuitBreakerId, ILogger log, IDurableClient durableClient)
        {
            log?.LogCircuitBreakerMessage(circuitBreakerId, $"Recording success for circuit-breaker = '{circuitBreakerId}'.");

            await durableClient.SignalEntityAsync<IDurableCircuitBreaker>(circuitBreakerId, breaker => breaker.RecordSuccess());
        }

        public async Task RecordFailure(string circuitBreakerId, ILogger log, IDurableClient durableClient)
        {
            log?.LogCircuitBreakerMessage(circuitBreakerId, $"Recording failure for circuit-breaker = '{circuitBreakerId}'.");

            await durableClient.SignalEntityAsync<IDurableCircuitBreaker>(circuitBreakerId, breaker => breaker.RecordFailure());
        }

        public async Task<CircuitState> GetCircuitState(string circuitBreakerId, ILogger log, IDurableClient durableClient)
        {
            log?.LogCircuitBreakerMessage(circuitBreakerId, $"Getting circuit state for circuit-breaker = '{circuitBreakerId}'.");

            var readState = await durableClient.ReadEntityStateAsync<DurableCircuitBreaker>(DurableCircuitBreaker.GetEntityId(circuitBreakerId));

            // To keep the return type simple, we present a not-yet-initialized circuit-breaker as closed (it will be closed when first used).
            return readState.EntityExists && readState.EntityState != null ? readState.EntityState.CircuitState : CircuitState.Closed;
        }

        public async Task<DurableCircuitBreaker> GetBreakerState(string circuitBreakerId, ILogger log, IDurableClient durableClient)
        {
            log?.LogCircuitBreakerMessage(circuitBreakerId, $"Getting breaker state for circuit-breaker = '{circuitBreakerId}'.");

            var readState = await durableClient.ReadEntityStateAsync<DurableCircuitBreaker>(DurableCircuitBreaker.GetEntityId(circuitBreakerId));

            // We present a not-yet-initialized circuit-breaker as null (it will be initialized when successes or failures are first posted against it).
            if (!readState.EntityExists || readState.EntityState == null)
            {
                return null;
            }

            return readState.EntityState;
        }

        private async Task<DurableCircuitBreaker> GetBreakerStateWithCaching(string circuitBreakerId, Func<Task<DurableCircuitBreaker>> getBreakerState, IConfiguration configuration)
        {
            var cachePolicy = GetCachePolicy(circuitBreakerId);
            var context = new Context($"{DurableCircuitBreakerKeyPrefix}{circuitBreakerId}");
            return await cachePolicy.ExecuteAsync(ctx => getBreakerState(), context);
        }

        private IAsyncPolicy<DurableCircuitBreaker> GetCachePolicy(string circuitBreakerId)
        {
            var key = $"{DurableCircuitBreakerKeyPrefix}{circuitBreakerId}";

            if (_policyRegistry.TryGet(key, out IAsyncPolicy<DurableCircuitBreaker> cachePolicy))
            {
                return cachePolicy;
            }

            var settings = _options.Get(circuitBreakerId);
            var checkCircuitInterval = settings.PerformancePriorityCheckCircuitIntervalTime;

            if (checkCircuitInterval > TimeSpan.Zero)
            {
                cachePolicy = Policy.CacheAsync(
                    _serviceProvider
                        .GetRequiredService<IAsyncCacheProvider>()
                        .AsyncFor<DurableCircuitBreaker>(),
                    checkCircuitInterval);
            }
            else
            {
                cachePolicy = Policy.NoOpAsync<DurableCircuitBreaker>();
            }

            _policyRegistry[key] = cachePolicy;

            return cachePolicy;
        }
    }
}