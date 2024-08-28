using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lueben.Microservice.CircuitBreaker.HealthCheck.Tests
{
    public class CircuitBreakerHealthCheckTests
    {
        private const string cbId = "test";

        public CircuitBreakerHealthCheckTests()
        {

        }

        [Fact]
        public async Task GivenCircuitBreaker_WhenItIsNotOpen_ThenStatusIsHealthy()
        {
            var cbCheckerMock = new Mock<ICircuitBreakerStateChecker>();
            var logger = new Mock<ILogger<CircuitBreakerHealthCheck>>();

            cbCheckerMock.Setup(x => x.IsCircuitBreakerInOpenState(new List<string> { cbId })).ReturnsAsync(false);

            var healthCheck = new CircuitBreakerHealthCheck(logger.Object, cbCheckerMock.Object, cbId);

            var result =  await healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.Equal(HealthStatus.Healthy, result.Status);
        }

        [Fact]
        public async Task GivenCircuitBreaker_WhenItIsOpen_ThenStatusIsDegraded()
        {
            var cbCheckerMock = new Mock<ICircuitBreakerStateChecker>();
            var logger = new Mock<ILogger<CircuitBreakerHealthCheck>>();

            cbCheckerMock.Setup(x => x.IsCircuitBreakerInOpenState(new List<string> { cbId })).ReturnsAsync(true);

            var healthCheck = new CircuitBreakerHealthCheck(logger.Object, cbCheckerMock.Object, cbId);

            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.Equal(HealthStatus.Degraded, result.Status);
        }

        [Fact]
        public async Task GivenCircuitBreaker_WhenItFails_ThenStatusIsUnhealthy()
        {
            var cbCheckerMock = new Mock<ICircuitBreakerStateChecker>();
            var logger = new Mock<ILogger<CircuitBreakerHealthCheck>>();

            cbCheckerMock.Setup(x => x.IsCircuitBreakerInOpenState(new List<string> { cbId })).Throws<Exception>();

            var healthCheck = new CircuitBreakerHealthCheck(logger.Object, cbCheckerMock.Object, cbId);

            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.Equal(HealthStatus.Unhealthy, result.Status);
        }
    }
}