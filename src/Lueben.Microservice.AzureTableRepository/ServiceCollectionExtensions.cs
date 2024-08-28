using System;
using System.Collections.Generic;
using Azure.Data.Tables;
using Lueben.Microservice.Diagnostics;
using Lueben.Microservice.Extensions.Configuration;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.AzureTableRepository
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureTableRepositoriesFromOptions(this IServiceCollection services, IList<Type> tableTypes)
        {
            Ensure.ArgumentNotNull(tableTypes, nameof(tableTypes));

            services.RegisterNamedOptions<AzureTableRepositoryOptions>();

            services.AddAzureClients(clientBuilder =>
            {
                foreach (var type in tableTypes)
                {
                    string connectionName = null;
                    clientBuilder.AddClient<TableServiceClient, TableClientOptions>((_, _, provider) =>
                    {
                        var tableOptions = provider.GetService<IOptionsSnapshot<AzureTableRepositoryOptions>>();
                        connectionName = tableOptions.Get(type.Name).Connection;
                        var configuration = provider.GetService<IConfiguration>();
                        var connectionString = configuration.GetConnectionStringWithFallBack(connectionName);
                        return new TableServiceClient(connectionString);
                    }).WithName(Helpers.GetTableServiceClientName(connectionName));
                }
            });

            services.AddTransient(typeof(AzureTableRepository<>));

            return services;
        }

        public static IServiceCollection AddAzureTableRepository<T>(this IServiceCollection services, AzureTableRepositoryOptions tableOptions = default)
            where T : class, ITableEntity, new()
        {
            services.AddAzureTableServiceClient(tableOptions?.Connection);

            var tableName = Helpers.GetTableName<T>(tableOptions);
            var clientName = Helpers.GetTableServiceClientName(tableOptions);

            return services.AddTransient(p => new AzureTableRepository<T>(
                    p.GetService<IAzureClientFactory<TableServiceClient>>().CreateClient(clientName),
                    tableName));
        }

        public static IServiceCollection AddAzureTableServiceClient(this IServiceCollection services, string connectionName = default)
        {
            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddClient<TableServiceClient, TableClientOptions>((options, credential, provider) =>
                {
                     var configuration = provider.GetService<IConfiguration>();
                     var connectionString = configuration.GetConnectionStringWithFallBack(connectionName);
                     Ensure.ArgumentNotNullOrEmpty(connectionString, nameof(connectionString));
                     return new TableServiceClient(connectionString);
                }).WithName(Helpers.GetTableServiceClientName(connectionName));
            });

            return services;
        }
    }
}