using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Lueben.Microservice.EventHub;
using Lueben.Microservice.EventHub.HealthCheck;
using Xunit;
using Moq;

namespace Lueben.Microservice.HealthChecks.Tests
{
    public class HealthChecksBuilderTests
    {
        private readonly IServiceCollection _serviceCollection;
        private IHealthChecksBuilder _healthChecksBuilder;

        public HealthChecksBuilderTests()
        {
            _serviceCollection = new ServiceCollection();
            _healthChecksBuilder = new HealthChecksBuilder(_serviceCollection);
        }

        [Fact]
        public void GivenAdd_WhenCalledWithNullRegistration_ThenShouldThrowArgumentException()
        {
            HealthCheckRegistration registration = null;

            Assert.Throws<ArgumentNullException>(() => _healthChecksBuilder.Add(registration));
        }

        [Fact]
        public async Task GivenAdd_WhenCalledWithRegistration_ThenHealthCheckServiceOptionsShouldBeRegistered()
        {
            var registration = new HealthCheckRegistration(
                "test",
                serviceProvider =>
                {
                    var mockOptions = new Mock<IOptionsSnapshot<EventHubOptions>>();
                    var mockEventHubHealthCheckService = new Mock<IEventHubHealthCheckService>();

                    return new EventHubHealthCheck(mockOptions.Object, mockEventHubHealthCheckService.Object);
                },
                HealthStatus.Unhealthy,
                default);

            _healthChecksBuilder.Add(registration);

            await using var provider = _serviceCollection.BuildServiceProvider();

            var optionsConfiguration = provider.GetService<IConfigureOptions<HealthCheckServiceOptions>>();

            Assert.NotNull(optionsConfiguration);

            var options = new HealthCheckServiceOptions();
            optionsConfiguration.Configure(options);

            Assert.NotNull(options.Registrations);
            Assert.Single(options.Registrations);
        }
    }
}
