using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Azure.Data.Tables;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.AzureTableRepository.HealthCheck
{
    [ExcludeFromCodeCoverage]
    public static class AzureTableHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddAzureTableHealthCheck<T>(this IHealthChecksBuilder builder,
            AzureTableRepositoryOptions options = default,
            string name = "AzureTableHealthCheck",
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default)
        where T : class
        {
            return builder.Add(new HealthCheckRegistration(
                name,
                serviceProvider =>
                {
                    options ??= serviceProvider.GetService<IOptionsSnapshot<AzureTableRepositoryOptions>>()?.Get(typeof(T).Name);
                    var tableServiceClientFactory = serviceProvider.GetRequiredService<IAzureClientFactory<TableServiceClient>>();
                    return new AzureTableHealthCheck<T>(tableServiceClientFactory, options);
                },
                failureStatus,
                tags));
        }
    }
}