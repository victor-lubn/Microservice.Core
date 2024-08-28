using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Lueben.Microservice.CircuitBreaker;

namespace Lueben.Microservice.Interceptors
{
    public class CircuitBreakerInterceptor : IAsyncInterceptor
    {
        private readonly string _circuitBreakerId;

        private readonly ICircuitBreakerClient _circuitBreakerClient;

        public CircuitBreakerInterceptor(ICircuitBreakerClient circuitBreakerClient, string circuitBreakerId)
        {
            _circuitBreakerClient = circuitBreakerClient ?? throw new ArgumentNullException(nameof(circuitBreakerClient));
            _circuitBreakerId = circuitBreakerId ?? throw new ArgumentNullException(nameof(circuitBreakerId));
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

        private async Task InternalInterceptAsynchronous(IInvocation invocation)
        {
            var capture = invocation.CaptureProceedInfo();

            await _circuitBreakerClient.Execute(_circuitBreakerId,
                async () =>
                {
                    capture.Invoke();
                    var task = (Task)invocation.ReturnValue;
                    await task;
                },
                () => throw new CircuitBreakerOpenStateException());
        }

        private async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation)
        {
            var capture = invocation.CaptureProceedInfo();

            return await _circuitBreakerClient.Execute(_circuitBreakerId,
                async () =>
                {
                    capture.Invoke();
                    var task = (Task<TResult>)invocation.ReturnValue;
                    return await task;
                },
                () => throw new CircuitBreakerOpenStateException());
        }
    }
}
