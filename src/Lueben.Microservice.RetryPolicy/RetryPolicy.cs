using System;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;

namespace Lueben.Microservice.RetryPolicy
{
    public class RetryPolicy<T> : IRetryPolicy<T>
        where T : Exception
    {
        public int RetryCount { get; }

        private readonly AsyncRetryPolicy _policy;

        public RetryPolicy(int retryCount, Func<T, bool> exceptionPredicate = null)
        {
            RetryCount = retryCount;
            var builder = exceptionPredicate != null ? Policy.Handle(exceptionPredicate) : Policy.Handle<T>();

            void OnRetry(Exception response, TimeSpan delay, int retryCountValue, Context context)
            {
                context[RetryPolicyConstants.RetryCountPropertyName] = retryCountValue;
            }

            _policy = builder.WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), onRetry: OnRetry);
        }

        public Task Execute(Func<Context, Task> action)
        {
            return _policy.ExecuteAsync(action, GetInitialContext());
        }

        public Task<TResult> Execute<TResult>(Func<Context, Task<TResult>> action)
        {
            return _policy.ExecuteAsync(action, GetInitialContext());
        }

        private static Context GetInitialContext()
        {
            var context = new Context
            {
                { RetryPolicyConstants.RetryCountPropertyName, 0 }
            };

            return context;
        }
    }
}
