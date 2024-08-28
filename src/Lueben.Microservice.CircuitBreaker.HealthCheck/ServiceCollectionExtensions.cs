using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

[assembly:ExcludeFromCodeCoverage]

namespace Lueben.Microservice.CircuitBreaker.HealthCheck
{
    public static class ServiceCollectionExtensions
    {
        public static IHealthChecksBuilder AddCircuitBreakerHealthCheck(
            this IHealthChecksBuilder builder,
            List<string> circuitBreakerIds,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default)
        {
            circuitBreakerIds.ForEach(circuitBreakerId => builder.AddTypeActivatedCheck<CircuitBreakerHealthCheck>(
                $"{circuitBreakerId} CircuitBreaker",
                failureStatus,
                tags,
                circuitBreakerId));

            return builder;
        }
    }
}
