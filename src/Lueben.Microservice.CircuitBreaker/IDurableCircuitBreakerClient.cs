using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lueben.Microservice.CircuitBreaker
{
    public interface IDurableCircuitBreakerClient
    {
        Task<bool> IsExecutionPermitted(string circuitBreakerId, ILogger log, IDurableClient durableClient, IConfiguration configuration);

        Task<bool> IsExecutionPermitted_StrongConsistency(string circuitBreakerId, ILogger log, IDurableClient orchestrationClient, IConfiguration configuration);

        Task RecordSuccess(string circuitBreakerId, ILogger log, IDurableClient durableClient);

        Task RecordFailure(string circuitBreakerId, ILogger log, IDurableClient durableClient);

        Task<CircuitState> GetCircuitState(string circuitBreakerId, ILogger log, IDurableClient durableClient);

        Task<DurableCircuitBreaker> GetBreakerState(string circuitBreakerId, ILogger log, IDurableClient durableClient);

        Task<bool> IsExecutionPermitted(string circuitBreakerId, ILogger log, IDurableOrchestrationContext orchestrationContext);

        Task RecordSuccess(string circuitBreakerId, ILogger log, IDurableOrchestrationContext orchestrationContext);

        Task RecordFailure(string circuitBreakerId, ILogger log, IDurableOrchestrationContext orchestrationContext);

        Task<CircuitState> GetCircuitState(string circuitBreakerId, ILogger log, IDurableOrchestrationContext orchestrationContext);

        Task<DurableCircuitBreaker> GetBreakerState(string circuitBreakerId, ILogger log, IDurableOrchestrationContext orchestrationContext);
    }
}