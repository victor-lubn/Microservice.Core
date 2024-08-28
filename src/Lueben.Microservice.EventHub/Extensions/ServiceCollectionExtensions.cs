using System;
using Azure.Core.Extensions;
using Azure.Identity;
using Azure.Messaging.EventHubs.Producer;
using Lueben.Microservice.Extensions.Configuration;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.EventHub.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventHubSender(this IServiceCollection services)
        {
            services
                .AddEventDataSender()
                .AddTransient<IEventDataSender, EventDataSender>();

            return services;
        }

        public static IServiceCollection AddEventDataSender(this IServiceCollection services)
        {
            services.RegisterConfiguration<EventHubOptions>(nameof(EventHubOptions));

            var provider = services.BuildServiceProvider();
            var eventHubOptions = provider.GetService<IOptions<EventHubOptions>>();

            services.AddAzureClients(builder =>
            {
                IAzureClientBuilder<EventHubProducerClient, EventHubProducerClientOptions> clientBuilder;

                if (eventHubOptions.Value.ConnectionString != null)
                {
                    clientBuilder = builder.AddEventHubProducerClient(eventHubOptions.Value.ConnectionString, eventHubOptions.Value.Name);
                }
                else
                {
                    if (string.IsNullOrEmpty(eventHubOptions.Value.Namespace) || string.IsNullOrEmpty(eventHubOptions.Value.Name))
                    {
                        throw new ArgumentException("Event hub connection parameters are not set.");
                    }

                    clientBuilder = builder.AddEventHubProducerClientWithNamespace(eventHubOptions.Value.Namespace, eventHubOptions.Value.Name);
                }

                clientBuilder.ConfigureOptions(options =>
                {
                    if (eventHubOptions.Value.MaxRetryCount.HasValue)
                    {
                        options.RetryOptions.MaximumRetries = eventHubOptions.Value.MaxRetryCount.Value;
                    }

                    if (eventHubOptions.Value.MaxRetryDelay.HasValue)
                    {
                        options.RetryOptions.MaximumDelay =
                            new TimeSpan(0, 0, eventHubOptions.Value.MaxRetryDelay.Value);
                    }
                });

                builder.UseCredential(new ManagedIdentityCredential());
            });

            return services;
        }
    }
}
