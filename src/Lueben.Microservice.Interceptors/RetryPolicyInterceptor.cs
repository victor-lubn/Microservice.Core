using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Lueben.Microservice.RetryPolicy;

namespace Lueben.Microservice.Interceptors
{
    public class RetryPolicyInterceptor<T> : IAsyncInterceptor
        where T : Exception
    {
        private readonly IRetryPolicy<T> _retryPolicy;
        private readonly Action<T> _postAction;

        protected int RetryCount { get; private set; }

        public RetryPolicyInterceptor(IRetryPolicy<T> retryPolicy, Action<T> postAction = null)
        {
            _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
            _postAction = postAction;
        }

        public void InterceptSynchronous(IInvocation invocation)
        {
            invocation.Proceed();
        }

        public void InterceptAsynchronous(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous(invocation);
        }

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous<TResult>(invocation);
        }

        protected virtual void ExecuteAfter()
        {
        }

        protected virtual void ExecuteBefore()
        {
        }

        private async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation)
        {
            ExecuteBefore();

            var capture = invocation.CaptureProceedInfo();
            var retryCount = 0;

            try
            {
                return await _retryPolicy.Execute(async context =>
                {
                    capture.Invoke();
                    retryCount = (int)context[RetryPolicyConstants.RetryCountPropertyName];
                    var task = (Task<TResult>)invocation.ReturnValue;
                    return await task;
                });
            }
            catch (T ex)
            {
                _postAction?.Invoke(ex);

                throw;
            }
            finally
            {
                ExecuteAfter();
            }
        }

        private async Task InternalInterceptAsynchronous(IInvocation invocation)
        {
            ExecuteBefore();

            var capture = invocation.CaptureProceedInfo();
            var retryCount = 0;

            try
            {
                await _retryPolicy.Execute(async context =>
                {
                    capture.Invoke();
                    retryCount = (int)context[RetryPolicyConstants.RetryCountPropertyName];
                    var task = (Task)invocation.ReturnValue;
                    await task;
                });
            }
            catch (T ex)
            {
                _postAction?.Invoke(ex);

                throw;
            }
            finally
            {
                ExecuteAfter();
            }
        }
    }
}