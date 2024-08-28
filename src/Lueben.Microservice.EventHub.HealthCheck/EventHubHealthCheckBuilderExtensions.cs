using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.EventHub.HealthCheck
{
    [ExcludeFromCodeCoverage]
    public static class EventHubHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddEventHubHealthCheck(
            this IHealthChecksBuilder builder,
            string name = "EventHubHealthCheck",
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name,
                serviceProvider => new EventHubHealthCheck(serviceProvider.GetRequiredService<IOptionsSnapshot<EventHubOptions>>(),
                serviceProvider.GetRequiredService<IEventHubHealthCheckService>()),
                failureStatus,
                tags));
        }
    }
}
