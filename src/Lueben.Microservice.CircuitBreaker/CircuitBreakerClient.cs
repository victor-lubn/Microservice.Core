using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.CircuitBreaker
{
    public class CircuitBreakerClient : ICircuitBreakerClient
    {
        private readonly IDurableCircuitBreakerClient _durableCircuitBreakerClient;
        private readonly ILogger<CircuitBreakerClient> _logger;
        private readonly IDurableClient _orchestrationClient;
        private readonly IConfiguration _configuration;

        public CircuitBreakerClient(IDurableClientFactory durableClientFactory, IDurableCircuitBreakerClient durableCircuitBreakerClient, ILogger<CircuitBreakerClient> logger, IConfiguration configuration, IOptions<DurableTaskOptions> options)
        {
            _durableCircuitBreakerClient = durableCircuitBreakerClient ?? throw new ArgumentNullException(nameof(durableCircuitBreakerClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _orchestrationClient = durableClientFactory.CreateClient(new DurableClientOptions
            {
                TaskHub = options.Value.HubName
            });
        }

        public async Task<T> Execute<T>(string circuitBreakerId, Func<Task<T>> action, Func<Task<T>> callback = null)
        {
            if (!await _durableCircuitBreakerClient.IsExecutionPermitted(circuitBreakerId, _logger, _orchestrationClient, _configuration))
            {
                _logger.LogWarning($"{circuitBreakerId} is in Open state. Calling callback function");
                if (callback != null)
                {
                    return await callback();
                }
            }

            try
            {
                var result = await action();
                await _durableCircuitBreakerClient.RecordSuccess(circuitBreakerId, _logger, _orchestrationClient);
                return result;
            }
            catch (RetriableOperationFailedException exception)
            {
                await _durableCircuitBreakerClient.RecordFailure(circuitBreakerId, _logger, _orchestrationClient);

                _logger.LogWarning($"{circuitBreakerId} recorded the exception: {exception.Message}");

                throw;
            }
            catch (Exception)
            {
                await _durableCircuitBreakerClient.RecordSuccess(circuitBreakerId, _logger, _orchestrationClient);
                throw;
            }
        }

        public async Task Execute(string circuitBreakerId, Func<Task> action, Func<Task> callback = null)
        {
            if (!await _durableCircuitBreakerClient.IsExecutionPermitted(circuitBreakerId, _logger, _orchestrationClient, _configuration))
            {
                _logger.LogWarning($"{circuitBreakerId} is in Open state. Calling callback function");
                if (callback != null)
                {
                    await callback();
                }
            }

            try
            {
                await action();
                await _durableCircuitBreakerClient.RecordSuccess(circuitBreakerId, _logger, _orchestrationClient);
            }
            catch (RetriableOperationFailedException exception)
            {
                await _durableCircuitBreakerClient.RecordFailure(circuitBreakerId, _logger, _orchestrationClient);

                _logger.LogWarning($"{circuitBreakerId} recorded the exception: {exception.Message}");

                throw;
            }
            catch (Exception)
            {
                await _durableCircuitBreakerClient.RecordSuccess(circuitBreakerId, _logger, _orchestrationClient);
                throw;
            }
        }
    }
}
