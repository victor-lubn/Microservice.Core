using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Logging;

namespace Lueben.Microservice.EventHub.HealthCheck
{
    [ExcludeFromCodeCoverage]
    public class EventHubHealthCheckService : IEventHubHealthCheckService
    {
        private readonly ILogger _logger;

        public EventHubHealthCheckService(ILogger<EventHubHealthCheckService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> IsAvailable(string eventHubNamespace, string eventHubName)
        {
            try
            {
                var tokenCredential = new ChainedTokenCredential(new ManagedIdentityCredential(), new VisualStudioCredential());
                var eventHubClient = new EventHubProducerClient(eventHubNamespace, eventHubName, tokenCredential);

                await eventHubClient.GetEventHubPropertiesAsync();
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception, $"Error occurred while getting access to service: {exception.Message}");
                return false;
            }
        }
    }
}
