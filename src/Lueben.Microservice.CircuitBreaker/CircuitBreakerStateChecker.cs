using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.CircuitBreaker
{
    public class CircuitBreakerStateChecker : ICircuitBreakerStateChecker
    {
        private readonly IDurableCircuitBreakerClient _durableCircuitBreakerClient;
        private readonly ILogger<CircuitBreakerStateChecker> _logger;
        private readonly IList<string> _circuitBrakerInstanceIds;
        private readonly IConfiguration _configuration;
        private readonly IDurableClient _orchestrationClient;

        public CircuitBreakerStateChecker(IDurableClientFactory durableClientFactory, IDurableCircuitBreakerClient durableCircuitBreakerClient, ILogger<CircuitBreakerStateChecker> logger, IConfiguration configuration, IOptions<DurableTaskOptions> options, IList<string> circuitBrakerInstanceIds = null)
        {
            _circuitBrakerInstanceIds = circuitBrakerInstanceIds;
            _durableCircuitBreakerClient = durableCircuitBreakerClient ?? throw new ArgumentNullException(nameof(durableCircuitBreakerClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _orchestrationClient = durableClientFactory.CreateClient(new DurableClientOptions
            {
                TaskHub = options.Value.HubName
            });
        }

        public async Task<bool> IsCircuitBreakerInOpenState()
        {
            return await CheckCircuitBreakersState(_circuitBrakerInstanceIds);
        }

        public async Task<bool> IsCircuitBreakerInOpenState(IList<string> circuitBreakerInstanceIds)
        {
            return await CheckCircuitBreakersState(circuitBreakerInstanceIds);
        }

        private async Task<bool> CheckCircuitBreakersState(IList<string> circuitBreakerInstanceIds)
        {
            foreach (var circuitBreakerInstanceId in circuitBreakerInstanceIds)
            {
                if (!await _durableCircuitBreakerClient.IsExecutionPermitted(circuitBreakerInstanceId, _logger, _orchestrationClient, _configuration))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
