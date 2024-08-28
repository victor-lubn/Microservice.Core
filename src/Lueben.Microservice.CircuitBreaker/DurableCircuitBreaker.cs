using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lueben.Microservice.CircuitBreaker
{
    [JsonObject(MemberSerialization.OptIn)]
    public partial class DurableCircuitBreaker : IDurableCircuitBreaker
    {
        private const string EntityName = nameof(DurableCircuitBreaker);

        private readonly ILogger _log;

        public DurableCircuitBreaker(ILogger log)
        {
            _log = log; // Do not throw for null; sometimes it will be initialized by the functions runtime with log == null, just to create a value instance to return. When operations are invoked, a logger is passed correctly.
        }

        public static EntityId GetEntityId(string circuitBreakerId) => new EntityId(EntityName, circuitBreakerId);

        [JsonProperty]
        public DateTime BrokenUntil { get; set; }

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public CircuitState CircuitState { get; set; }

        [JsonProperty]
        public int ConsecutiveFailureCount { get; set; }

        [JsonProperty]
        public int MaxConsecutiveFailures { get; set; }

        [JsonProperty]
        public TimeSpan BreakDuration { get; set; }

        public Task<bool> IsExecutionPermitted()
        {
            var circuitBreakerId = Entity.Current.EntityKey;

            switch (CircuitState)
            {
                case CircuitState.Closed:
                    return Task.FromResult(true);

                case CircuitState.Open:
                case CircuitState.HalfOpen:

                    // When the breaker is Open or HalfOpen, we permit a single test execution after BreakDuration has passed.
                    // The test execution phase is known as HalfOpen state.
                    if (DateTime.UtcNow > BrokenUntil)
                    {
                        _log?.LogCircuitBreakerMessage(circuitBreakerId, $"Permitting a test execution in half-open state: {circuitBreakerId}.");

                        CircuitState = CircuitState.HalfOpen;
                        BrokenUntil = DateTime.UtcNow + BreakDuration;

                        return Task.FromResult(true);
                    }

                    // - Not time yet to test the circuit again.
                    return Task.FromResult(false);

                default:
                    throw new InvalidOperationException();
            }
        }

        public Task<CircuitState> RecordSuccess()
        {
            string circuitBreakerId = Entity.Current.EntityKey;

            ConsecutiveFailureCount = 0;

            // A success result in HalfOpen state causes the circuit to close (permit executions) again.
            if (IsHalfOpen())
            {
                _log?.LogCircuitBreakerMessage(circuitBreakerId, $"Circuit re-closing: {circuitBreakerId}.");

                BrokenUntil = DateTime.MinValue;
                CircuitState = CircuitState.Closed;
            }

            return Task.FromResult(CircuitState);
        }

        public Task<CircuitState> RecordFailure()
        {
            string circuitBreakerId = Entity.Current.EntityKey;

            ConsecutiveFailureCount++;

            // If we have too many consecutive failures, open the circuit.
            // Or a failure when in the HalfOpen 'testing' state? That also breaks the circuit again.
            if ((CircuitState == CircuitState.Closed && ConsecutiveFailureCount >= MaxConsecutiveFailures) || IsHalfOpen())
            {
                _log?.LogCircuitBreakerMessage(circuitBreakerId, $"Circuit {(IsHalfOpen() ? "re-opening" : "opening")}: {circuitBreakerId}.");

                CircuitState = CircuitState.Open;
                BrokenUntil = DateTime.UtcNow + BreakDuration;
            }

            return Task.FromResult(CircuitState);
        }

        public Task<CircuitState> GetCircuitState()
        {
            return Task.FromResult(CircuitState);
        }

        public Task<DurableCircuitBreaker> GetBreakerState()
        {
            return Task.FromResult(this);
        }

        private bool IsHalfOpen()
        {
            return CircuitState == CircuitState.HalfOpen || (CircuitState == CircuitState.Open && DateTime.UtcNow > BrokenUntil);
        }
    }
}