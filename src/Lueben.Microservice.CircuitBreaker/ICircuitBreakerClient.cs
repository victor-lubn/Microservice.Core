using System;
using System.Threading.Tasks;

namespace Lueben.Microservice.CircuitBreaker
{
    public interface ICircuitBreakerClient
    {
        Task<T> Execute<T>(string circuitBreakerId, Func<Task<T>> action, Func<Task<T>> callback = null);

        Task Execute(string circuitBreakerId, Func<Task> action, Func<Task> callback = null);
    }
}
