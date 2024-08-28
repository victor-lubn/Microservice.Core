using Azure.Data.Tables;
using Lueben.Microservice.Extensions.Configuration;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.CircuitBreaker.CleanUp
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCircuitBreakerCleanUp(this IServiceCollection services)
        {
            services.RegisterConfiguration<EntityCleanUpOptions>("CircuitBreakerCleanUpOptions")
                .AddTransient<IEntityAzureStorageCleaner, EntityAzureStorageCleaner>();

            services.AddAzureClients(clientBuilder => clientBuilder.AddClient<TableServiceClient, TableClientOptions>(
                (_, _, provider) =>
                {
                    var configuration = provider.GetService<IConfiguration>();
                    var connectionString = configuration.GetValue<string>(Constants.DefaultConnectionName);
                    return new TableServiceClient(connectionString);
                }).WithName(Constants.CircuitBreakerHistoryTableClient));

            return services;
        }
    }
}