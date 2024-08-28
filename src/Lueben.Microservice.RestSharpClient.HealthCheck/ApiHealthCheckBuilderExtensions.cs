using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Lueben.Microservice.RestSharpClient.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Lueben.Microservice.RestSharpClient.HealthCheck
{
    [ExcludeFromCodeCoverage]
    public static class ApiHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddApiHealthCheck(
            this IHealthChecksBuilder builder,
            string url,
            string sectionName,
            string name = "ApiHealthCheck",
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name,
                serviceProvider =>
                {
                    var factory = serviceProvider.GetRequiredService<IRestSharpClientFactory>();
                    var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<ApiHealthCheck>();
                    return new ApiHealthCheck(factory, logger, url, sectionName);
                },
                failureStatus,
                tags));
        }
    }
}