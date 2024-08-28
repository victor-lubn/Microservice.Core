using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.DurableFunction.HealthCheck
{
    [ExcludeFromCodeCoverage]
    public static class WorkflowHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddWorkflowHealthCheck(
            this IHealthChecksBuilder builder,
            string instanceId = null,
            string name = "workflow",
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name,
                serviceProvider =>
                {
                    var durableClientFactory = serviceProvider.GetRequiredService<IDurableClientFactory>();
                    var taskOptions = serviceProvider.GetRequiredService<IOptionsSnapshot<DurableTaskOptions>>();
                    var healthCheckOptions = serviceProvider.GetRequiredService<IOptionsSnapshot<WorkflowHealthCheckOptions>>();
                    var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<WorkflowHealthCheck>();
                    return new WorkflowHealthCheck(durableClientFactory, healthCheckOptions, taskOptions, logger, instanceId);
                },
                failureStatus,
                tags));
        }
    }
}