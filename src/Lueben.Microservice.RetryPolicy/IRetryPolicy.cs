using System;
using System.Threading.Tasks;
using Polly;

namespace Lueben.Microservice.RetryPolicy
{
    public interface IRetryPolicy<T>
        where T : Exception
    {
        Task Execute(Func<Context, Task> action);

        Task<TResult> Execute<TResult>(Func<Context, Task<TResult>> action);
    }
}
