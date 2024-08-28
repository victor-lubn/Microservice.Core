using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.EventHub.HealthCheck
{
    public class EventHubHealthCheck : IHealthCheck
    {
        private readonly IOptionsSnapshot<EventHubOptions> _options;
        private readonly IEventHubHealthCheckService _eventHubHealthCheckService;

        public EventHubHealthCheck(IOptionsSnapshot<EventHubOptions> options, IEventHubHealthCheckService eventHubHealthCheckService)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _eventHubHealthCheckService = eventHubHealthCheckService ?? throw new ArgumentException(nameof(eventHubHealthCheckService));

            if (string.IsNullOrEmpty(options.Value.Namespace) || string.IsNullOrEmpty(options.Value.Name))
            {
                throw new ArgumentException("Event hub connection parameters are not set.");
            }
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var result = await _eventHubHealthCheckService.IsAvailable(_options.Value.Namespace, _options.Value.Name);

            return result ? HealthCheckResult.Healthy("Service is available.") :
                HealthCheckResult.Unhealthy("Service is not available.");
        }
    }
}