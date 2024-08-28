using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Lueben.Microservice.AzureTableRepository.HealthCheck
{
    public class AzureTableHealthCheck<T> : IHealthCheck
    {
        private readonly IAzureClientFactory<TableServiceClient> _factory;
        private readonly AzureTableRepositoryOptions _options;

        public AzureTableHealthCheck(IAzureClientFactory<TableServiceClient> factory,  AzureTableRepositoryOptions options)
        {
            _factory = factory;
            _options = options;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = _factory.CreateClient(Helpers.GetTableServiceClientName(_options));
                await client.CreateTableIfNotExistsAsync(Helpers.GetTableName<T>(_options), cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}