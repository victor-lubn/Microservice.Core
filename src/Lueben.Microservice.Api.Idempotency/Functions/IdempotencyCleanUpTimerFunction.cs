using System;
using System.Threading.Tasks;

using Lueben.Microservice.Api.Idempotency.IdempotencyDataProviders;
using Lueben.Microservice.Api.Idempotency.Models;
using Lueben.Microservice.Diagnostics;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Lueben.Microservice.Api.Idempotency.Functions
{
    public class IdempotencyCleanUpTimerFunction
    {
        private readonly IIdempotencyDataProvider<IdempotencyEntity> _dataProvider;

        public IdempotencyCleanUpTimerFunction(IIdempotencyDataProvider<IdempotencyEntity> dataProvider)
        {
            Ensure.ArgumentNotNull(dataProvider, nameof(dataProvider));

            _dataProvider = dataProvider;
        }

        [Function(nameof(TriggerIdempotencyCleanUpTimer))]
        public async Task TriggerIdempotencyCleanUpTimer(
            [TimerTrigger("0 0 23 * * *")] TimerInfo timer, ILogger log)
        {
            if (timer.IsPastDue)
            {
                log.LogInformation($"Idempotency timer is running late: {DateTime.UtcNow}");
            }

            log.LogInformation($"Idempotency cleanup timer trigger function executed at: {DateTime.UtcNow}");

            await _dataProvider.CleanUp();
        }
    }
}
