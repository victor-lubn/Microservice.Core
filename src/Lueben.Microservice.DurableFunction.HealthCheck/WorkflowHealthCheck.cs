using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.DurableFunction.HealthCheck
{
    public class WorkflowHealthCheck : IHealthCheck
    {
        private readonly IDurableClientFactory _durableClientFactory;
        private readonly IOptionsSnapshot<WorkflowHealthCheckOptions> _healthCheckOptions;
        private readonly IOptionsSnapshot<DurableTaskOptions> _taskOptions;
        private readonly ILogger _logger;
        private readonly string _instanceId;

        public WorkflowHealthCheck(IDurableClientFactory durableClientFactory,
            IOptionsSnapshot<WorkflowHealthCheckOptions> healthCheckOptions,
            IOptionsSnapshot<DurableTaskOptions> taskOptions,
            ILogger<WorkflowHealthCheck> logger,
            string instanceId)
        {
            _durableClientFactory = durableClientFactory ?? throw new ArgumentNullException(nameof(durableClientFactory));
            _healthCheckOptions = healthCheckOptions ?? throw new ArgumentNullException(nameof(healthCheckOptions));
            _taskOptions = taskOptions ?? throw new ArgumentNullException(nameof(taskOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _instanceId = instanceId;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var durableClient = _durableClientFactory.CreateClient(new DurableClientOptions
                {
                    TaskHub = _taskOptions.Value.HubName
                });

                if (string.IsNullOrEmpty(_instanceId))
                {
                    return await MultipleHealthCheckResult(durableClient);
                }

                return await SingleHealthCheckResult(durableClient);
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception, $"HealthCheck for workflow {_instanceId} failed with message: '{exception.Message}'.");
            }

            return HealthCheckResult.Unhealthy("Failed to check status of the workflow.");
        }

        private static string SerializeStatus(DurableOrchestrationStatus status)
        {
            return $"{status.RuntimeStatus}: created {status.CreatedTime}; last updated {status.LastUpdatedTime}.";
        }

        private async Task<HealthCheckResult> MultipleHealthCheckResult(IDurableClient durableClient)
        {
            var maxDays = _healthCheckOptions.Value.MaxDaysSinceLastUpdated;
            string continuationToken = null;
            var totalRunningInstances = 0;
            var totalPendingInstances = 0;
            do
            {
                var result = await durableClient.ListInstancesAsync(new OrchestrationStatusQueryCondition
                    {
                        PageSize = _healthCheckOptions.Value.HistoryPageSize,
                        ContinuationToken = continuationToken,
                        RuntimeStatus = new[] { OrchestrationRuntimeStatus.Pending, OrchestrationRuntimeStatus.Running },
                        CreatedTimeFrom = DateTime.Now.AddDays(-_healthCheckOptions.Value.CreatedDaysFrom)
                    },
                    cancellationToken: CancellationToken.None);

                if (result?.DurableOrchestrationState == null && continuationToken == null)
                {
                    return HealthCheckResult.Healthy("No history data.");
                }

                if (result?.DurableOrchestrationState != null)
                {
                    var longRunning = result.DurableOrchestrationState.FirstOrDefault(s => s.IsOverdue(maxDays) && !s.IsEntityOrchestration());
                    if (longRunning != null)
                    {
                        return HealthCheckResult.Unhealthy($"In state to long {longRunning.InstanceId} - " + SerializeStatus(longRunning));
                    }

                    totalRunningInstances += result.DurableOrchestrationState.Count(s => s.RuntimeStatus == OrchestrationRuntimeStatus.Running);
                    totalPendingInstances += result.DurableOrchestrationState.Count(s => s.RuntimeStatus == OrchestrationRuntimeStatus.Pending);
                }

                continuationToken = result?.ContinuationToken;
            }
            while (!string.IsNullOrEmpty(continuationToken));

            return HealthCheckResult.Healthy($"Running {totalRunningInstances}; Pending {totalPendingInstances}.");
        }

        private async Task<HealthCheckResult> SingleHealthCheckResult(IDurableOrchestrationClient durableClient)
        {
            var status = await durableClient.GetStatusAsync(_instanceId);

            if (status == null)
            {
                return HealthCheckResult.Healthy("No workflow history.");
            }

            switch (status.RuntimeStatus)
            {
                case OrchestrationRuntimeStatus.ContinuedAsNew:
                case OrchestrationRuntimeStatus.Unknown:
                case OrchestrationRuntimeStatus.Terminated:
                case OrchestrationRuntimeStatus.Canceled:
                case OrchestrationRuntimeStatus.Completed:
                    return HealthCheckResult.Healthy(SerializeStatus(status));
                case OrchestrationRuntimeStatus.Failed:
                    return HealthCheckResult.Unhealthy(SerializeStatus(status) + $";{status.Output}");
                case OrchestrationRuntimeStatus.Running:
                case OrchestrationRuntimeStatus.Pending:
                    if (status.LastUpdatedTime < DateTime.Now.AddDays(-_healthCheckOptions.Value.MaxDaysSinceLastUpdated))
                    {
                        return HealthCheckResult.Unhealthy("In state to long - " + SerializeStatus(status));
                    }
                    else
                    {
                        return HealthCheckResult.Healthy(SerializeStatus(status));
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}