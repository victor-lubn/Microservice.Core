using System;
using System.Diagnostics.CodeAnalysis;
using Lueben.Microservice.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.RetryPolicy
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        private static readonly int MaxRetryCount = new RetryPolicyOptions().MaxRetryCount;

        public static IServiceCollection AddRetryPolicy(this IServiceCollection services)
        {
            return services.RegisterNamedOptions<RetryPolicyOptions>();
        }

        public static IServiceCollection AddRetryPolicy<T>(this IServiceCollection services, Func<T, bool> predicate = null)
            where T : Exception
        {
            return services.AddTransient<IRetryPolicy<T>, RetryPolicy<T>>(p =>
            {
                var configuration = p.GetService<IOptionsSnapshot<RetryPolicyOptions>>();
                var retryCount = configuration?.Get(typeof(T).Name)?.MaxRetryCount ?? MaxRetryCount;
                return new RetryPolicy<T>(retryCount, predicate);
            });
        }
    }
}