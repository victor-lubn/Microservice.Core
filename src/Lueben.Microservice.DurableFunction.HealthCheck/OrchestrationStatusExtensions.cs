using System;
using System.Text.RegularExpressions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Lueben.Microservice.DurableFunction.HealthCheck
{
    public static class OrchestrationStatusExtensions
    {
        public static readonly Regex EntityIdRegex = new Regex(@"@(\w+)@(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool IsEntityOrchestration(this DurableOrchestrationStatus status)
        {
            return EntityIdRegex.IsMatch(status.InstanceId ?? string.Empty);
        }

        public static double RunningDays(this DurableOrchestrationStatus status)
        {
            return (status.LastUpdatedTime - status.CreatedTime).TotalDays;
        }

        public static double DaysSinceUpdated(this DurableOrchestrationStatus status)
        {
            return (DateTime.Now - status.LastUpdatedTime).TotalDays;
        }

        public static bool IsOverdue(this DurableOrchestrationStatus status, double days)
        {
            return status.RunningDays() > days || status.DaysSinceUpdated() > days;
        }
    }
}