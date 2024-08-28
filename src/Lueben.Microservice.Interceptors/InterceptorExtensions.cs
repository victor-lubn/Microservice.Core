using System;
using Castle.DynamicProxy;
using Lueben.Microservice.CircuitBreaker;
using Lueben.Microservice.RetryPolicy;

namespace Lueben.Microservice.Interceptors
{
    public static class InterceptorExtensions
    {
        public static readonly Lazy<ProxyGenerator> ProxyGenerator = new Lazy<ProxyGenerator>(() => new ProxyGenerator());

        public static T AddRetryPolicy<T, TException>(this T restSharpClient, IRetryPolicy<TException> retryPolicy, Action<TException> postAction = null)
            where T : class
            where TException : Exception
        {
            var retryPolicyInterceptor = new RetryPolicyInterceptor<TException>(retryPolicy, postAction);
            return restSharpClient.AddInterceptor(retryPolicyInterceptor);
        }

        public static T AddCircuitBreaker<T>(this T client, ICircuitBreakerClient circuitBreakerClient, string id)
            where T : class
        {
            var circuitBreakerInterceptor = new CircuitBreakerInterceptor(circuitBreakerClient, id);
            return client.AddInterceptor(circuitBreakerInterceptor);
        }

        public static T AddInterceptor<T>(this T restSharpClient, IAsyncInterceptor interceptor)
            where T : class
        {
            return ProxyGenerator.Value.CreateInterfaceProxyWithTarget(restSharpClient, interceptor);
        }
    }
}