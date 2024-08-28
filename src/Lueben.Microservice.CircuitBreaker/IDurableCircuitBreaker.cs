using System.Threading.Tasks;

namespace Lueben.Microservice.CircuitBreaker
{
    public interface IDurableCircuitBreaker
    {
        Task<bool> IsExecutionPermitted();

        Task<CircuitState> RecordSuccess();

        Task<CircuitState> RecordFailure();

        Task<CircuitState> GetCircuitState();

        Task<DurableCircuitBreaker> GetBreakerState();
    }
}
