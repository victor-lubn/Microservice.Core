using Lueben.Microservice.EventHub;
using Lueben.Microservice.EventHub.HealthCheck;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Lueben.Microservice.HealthChecks.Tests
{
    public class EventHubHealthCheckTests
    {
        private readonly Mock<IOptionsSnapshot<EventHubOptions>> _mockOptions;

        public EventHubHealthCheckTests()
        {
            var options = Options.Create(new EventHubOptions
            {
                Name = "name",
                Namespace = "namespace"
            });
            _mockOptions = new Mock<IOptionsSnapshot<EventHubOptions>>();
            _mockOptions.Setup(x => x.Value)
                .Returns(options.Value);
        }

        [Fact]
        public async Task GivenEventHubHealthCheck_WhenServiceExecutesIt_ThenTheHealthyStatusShouldBeReturned()
        {
            var mockEventHubHealthCheckService = new Mock<IEventHubHealthCheckService>();
            mockEventHubHealthCheckService.Setup(x => x.IsAvailable(_mockOptions.Object.Value.Namespace, _mockOptions.Object.Value.Name))
                .Returns(Task.FromResult(true));

            var eventHubHealthCheck = new EventHubHealthCheck(_mockOptions.Object, mockEventHubHealthCheckService.Object);
            var result = await eventHubHealthCheck.CheckHealthAsync(null);

            Assert.Equal("Healthy", result.Status.ToString());
            Assert.Equal("Service is available.", result.Description);
        }

        [Fact]
        public async Task GivenEventHubHealthCheck_WhenEventHubHealthCheckServiceThrowsAnException_ThenTheUnhealthyStatusShouldBeReturned()
        {
            var mockEventHubHealthCheckService = new Mock<IEventHubHealthCheckService>();
            mockEventHubHealthCheckService.Setup(x => x.IsAvailable(_mockOptions.Object.Value.Namespace, _mockOptions.Object.Value.Name))
                .Returns(Task.FromResult(false));

            var eventHubHealthCheck = new EventHubHealthCheck(_mockOptions.Object, mockEventHubHealthCheckService.Object);

            var result = await eventHubHealthCheck.CheckHealthAsync(null);

            Assert.Equal("Unhealthy", result.Status.ToString());
            Assert.Equal("Service is not available.", result.Description);
        }
    }
}
