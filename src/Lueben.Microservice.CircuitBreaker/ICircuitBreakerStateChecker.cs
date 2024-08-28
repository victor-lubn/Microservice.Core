using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lueben.Microservice.CircuitBreaker
{
    public interface ICircuitBreakerStateChecker
    {
        Task<bool> IsCircuitBreakerInOpenState();

        Task<bool> IsCircuitBreakerInOpenState(IList<string> circuitBreakerInstanceIds);
    }
}
