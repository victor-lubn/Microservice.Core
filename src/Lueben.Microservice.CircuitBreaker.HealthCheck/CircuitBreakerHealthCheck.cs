using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lueben.Microservice.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Lueben.Microservice.CircuitBreaker.HealthCheck
{
    public class CircuitBreakerHealthCheck : IHealthCheck
    {
        private readonly ILogger<CircuitBreakerHealthCheck> _logger;
        private readonly ICircuitBreakerStateChecker _circuitBreakerStateChecker;
        private readonly string _circuitBreakerId;

        public CircuitBreakerHealthCheck(
            ILogger<CircuitBreakerHealthCheck> logger,
            ICircuitBreakerStateChecker circuitBreakerStateChecker,
            string circuitBreakerInstanceIdsConstant)
        {
            Ensure.ArgumentNotNull(logger, nameof(logger));

            _logger = logger;
            _circuitBreakerStateChecker = circuitBreakerStateChecker;
            _circuitBreakerId = circuitBreakerInstanceIdsConstant;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var isCircuitBreakerOpen = await _circuitBreakerStateChecker.IsCircuitBreakerInOpenState(new List<string> { _circuitBreakerId });

                return isCircuitBreakerOpen
                    ? HealthCheckResult.Degraded($"CircuitBreaker '{_circuitBreakerId}' is in Open state.")
                    : HealthCheckResult.Healthy($"Service is available.");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"CircuitBreakerHealthCheck for the {_circuitBreakerId} failed with the message: {ex.Message}");
                return HealthCheckResult.Unhealthy("Service is not available.");
            }
        }
    }
}
