using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DurableTask.Core;
using Lueben.Microservice.Diagnostics;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.DurableFunction.CleanUp
{
    [ExcludeFromCodeCoverage]
    public class DurableFunctionHistoryCleanUpFunction
    {
        private const string InstanceId = "E15B0322-105E-4018-815C-A7FFB2C7B659";
        private const string DateFormat = "MM/dd/yyyy H:mm";
        private readonly IOptionsSnapshot<DurableTaskOptions> _taskOptions;
        private readonly IOptionsSnapshot<DurableFunctionHistoryCleanUpOptions> _historyCleanUpOptions;
        private readonly ILogger<DurableFunctionHistoryCleanUpFunction> _logger;

        public DurableFunctionHistoryCleanUpFunction(IOptionsSnapshot<DurableTaskOptions> taskOptions, ILogger<DurableFunctionHistoryCleanUpFunction> logger, IOptionsSnapshot<DurableFunctionHistoryCleanUpOptions> historyCleanUpOptions)
        {
            Ensure.ArgumentNotNull(taskOptions, nameof(taskOptions));
            Ensure.ArgumentNotNull(historyCleanUpOptions, nameof(historyCleanUpOptions));
            Ensure.ArgumentNotNull(logger, nameof(logger));

            _taskOptions = taskOptions;
            _logger = logger;
            _historyCleanUpOptions = historyCleanUpOptions;
        }

        [FunctionName("PurgeInstanceHistory")]
        public async Task PurgeInstanceHistory([TimerTrigger("%DurableFunctionHistoryCleanUpTimerScheduleExpression%")] TimerInfo timerInfo, [DurableClient] IDurableOrchestrationClient durableClient)
        {
            _logger.LogInformation($"{nameof(DurableFunctionHistoryCleanUpFunction)} Timer trigger function started at: {DateTime.Now}");
            var startScanningDate = DateTime.UtcNow.AddMonths(-_historyCleanUpOptions.Value.MaxHistoryAgeMonths);
            var endScanningDate = startScanningDate.AddHours(_historyCleanUpOptions.Value.PurgeHistoryBatchTimeFrameHours);

            await durableClient.StartNewAsync(nameof(HistoryCleanUpOrchestrator), instanceId: InstanceId, (startScanningDate, endScanningDate));
        }

        [FunctionName(nameof(HistoryCleanUpOrchestrator))]
        public async Task HistoryCleanUpOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var (startScanningDate, endScanningDate) = context.GetInput<(DateTime, DateTime)>();
            var expirationTimeInDays = _historyCleanUpOptions.Value.HistoryExpirationDays;
            var toDate = context.CurrentUtcDateTime.AddDays(-expirationTimeInDays);

            PurgeHistoryResult result = null;
            try
            {
                result = await context.CallActivityAsync<PurgeHistoryResult>(nameof(HistoryCleanUpActivity), (startScanningDate, endScanningDate));
            }
            catch (FunctionFailedException wrappingException) when (wrappingException.InnerException is FunctionTimeoutException e)
            {
                _logger.LogWarning(
                    $"Purge of '{_taskOptions.Value.HubName}' history for {startScanningDate.ToString(DateFormat)}-{endScanningDate.ToString(DateFormat)} time range was incomplete due to error: {e.Message}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unhandled error. Failed to purge history of durable function for '{_taskOptions.Value.HubName}'.");
                throw;
            }

            _logger.LogInformation(result?.InstancesDeleted > 0
                ? $"'{result.InstancesDeleted}' records older '{expirationTimeInDays}' days were removed during '{nameof(DurableFunctionHistoryCleanUpFunction)}' run"
                : $"No records older '{expirationTimeInDays}' days were removed during '{nameof(DurableFunctionHistoryCleanUpFunction)}' run");

            if (endScanningDate < toDate)
            {
                startScanningDate = endScanningDate;
                endScanningDate = startScanningDate.AddHours(_historyCleanUpOptions.Value.PurgeHistoryBatchTimeFrameHours);
                context.ContinueAsNew((startScanningDate, endScanningDate));
            }
        }

        [FunctionName(nameof(HistoryCleanUpActivity))]
        public async Task<PurgeHistoryResult> HistoryCleanUpActivity([ActivityTrigger] (DateTime StartScanningDate, DateTime EndScanningDate) timeFrame, [DurableClient] IDurableOrchestrationClient client)
        {
            try
            {
                return await client.PurgeInstanceHistoryAsync(timeFrame.StartScanningDate, timeFrame.EndScanningDate, new[] { OrchestrationStatus.Completed });
            }
            catch (Exception exception)
            {
                _logger.LogWarning($"Attempt to cleanup the history of durable function for '{_taskOptions.Value.HubName}' failed with message: '{exception.Message}'.");

                return null;
            }
        }
    }
}