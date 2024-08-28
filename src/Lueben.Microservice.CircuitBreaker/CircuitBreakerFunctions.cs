using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.CircuitBreaker
{
    public class CircuitBreakerFunctions
    {
        private const string IsExecutionPermittedInternalOrchestratorName = "IsExecutionPermittedInternalOrchestratorName";
        private const string EntityName = nameof(DurableCircuitBreaker);

        private readonly OptionsManager<CircuitBreakerSettings> _options;

        public CircuitBreakerFunctions(OptionsManager<CircuitBreakerSettings> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Function entry point; d.
        /// </summary>
        /// <param name="context">An <see cref="IDurableEntityContext"/>, provided by dependency-injection.</param>
        /// <param name="logger">An <see cref="ILogger"/>, provided by dependency-injection.</param>
        [FunctionName(EntityName)]
        public async Task Run([EntityTrigger] IDurableEntityContext context, ILogger logger)
        {
            // The first time the circuit-breaker is accessed, it will self-configure.
            if (!context.HasState)
            {
                context.SetState(ConfigureCircuitBreaker(Entity.Current, logger));
            }

            await context.DispatchAsync<DurableCircuitBreaker>(logger);
        }

        [FunctionName(IsExecutionPermittedInternalOrchestratorName)]
        public Task<bool> IsExecutionPermittedInternalOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            string breakerId = context.GetInput<string>();
            if (string.IsNullOrEmpty(breakerId))
            {
                throw new InvalidOperationException($"{IsExecutionPermittedInternalOrchestratorName}: Could not determine breakerId of circuit-breaker requested.");
            }

            return IsExecutionPermitted(breakerId, log, context);
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

        private DurableCircuitBreaker ConfigureCircuitBreaker(IDurableEntityContext context, ILogger log)
        {
            string circuitBreakerId = context?.EntityKey;
            log?.LogCircuitBreakerMessage(circuitBreakerId, $"Setting configuration for circuit-breaker {circuitBreakerId}.");

            var settings = _options.Get(circuitBreakerId);
            var breaker = new DurableCircuitBreaker(null)
            {
                CircuitState = CircuitState.Closed,
                BrokenUntil = DateTime.MinValue,
                ConsecutiveFailureCount = 0,
                MaxConsecutiveFailures = settings.MaxConsecutiveFailures,
                BreakDuration = settings.BreakDurationTime
            };

            if (breaker.BreakDuration <= TimeSpan.Zero)
            {
                throw new InvalidOperationException($"Circuit-breaker {circuitBreakerId} must be configured with a positive break-duration.");
            }

            if (breaker.MaxConsecutiveFailures <= 0)
            {
                throw new InvalidOperationException($"Circuit-breaker {circuitBreakerId}  must be configured with a max number of consecutive failures greater than or equal to 1.");
            }

            return breaker;
        }
    }
}
