using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.CircuitBreaker.CleanUp
{
    public class CircuitBreakerCleanUpFunction
    {
        private const string DefaultCircuitBreakerEntityName = "durablecircuitbreaker";

        private readonly ILogger<CircuitBreakerCleanUpFunction> _logger;
        private readonly IEntityAzureStorageCleaner _entityAzureStorageCleaner;
        private readonly IOptionsSnapshot<EntityCleanUpOptions> _options;

        public CircuitBreakerCleanUpFunction(ILogger<CircuitBreakerCleanUpFunction> logger,
            IEntityAzureStorageCleaner entityAzureStorageCleaner,
            IOptionsSnapshot<EntityCleanUpOptions> options)
        {
            _logger = logger;
            _entityAzureStorageCleaner = entityAzureStorageCleaner;
            _options = options;
        }

        [FunctionName(nameof(CircuitBreakerMaintenance))]
        public async Task CircuitBreakerMaintenance(
            [TimerTrigger("%CircuitBreakerCleanUpTimerScheduleExpression%")] TimerInfo timerInfo)
        {
            _logger.LogInformation($"{nameof(CircuitBreakerCleanUpFunction)} Timer trigger function started at: {DateTime.UtcNow}");

            _options.Value.EntityName ??= DefaultCircuitBreakerEntityName;

            await _entityAzureStorageCleaner.CleanEntityHistory(_options.Value);
        }
    }
}