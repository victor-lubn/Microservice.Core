using System.Linq;
using System.Threading.Tasks;
using Lueben.Microservice.HealthChecks.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Lueben.Microservice.HealthChecks
{
    public class HealthCheckFunction
    {
        private readonly HealthCheckService _healthCheck;

        public HealthCheckFunction(HealthCheckService healthCheck)
        {
            _healthCheck = healthCheck;
        }

        [Function("Healthcheck")]
        public async Task<IActionResult> Healthcheck(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "healthcheck")] HttpRequest request,
           ILogger log)
        {
            log.Log(LogLevel.Information, "Received healthcheck request");

            var report = await _healthCheck.CheckHealthAsync(null);

            var response = new HealthCheckResponse
            {
                Status = report.Status.ToString(),
                Checks = report.Entries.Select(x => new HealthCheck
                {
                    Component = x.Key,
                    Status = x.Value.Status.ToString(),
                    Description = x.Value.Description
                })
            };

            foreach (var entry in report.Entries)
            {
                if (entry.Value.Exception != null)
                {
                    log.LogError(entry.Value.Exception.Message, $"Error occured while processing health check request");
                }
            }

            return new Models.HealthCheckResult { HealthCheckResponse = response };
        }
    }
}
