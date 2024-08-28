using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Lueben.Microservice.HealthChecks.Tests
{
    public class DefaultHealthCheckServiceTests
    {
        [Fact]
        public void GivenDefaultHealthCheckService_WhenDuplicateRegistrationsExist_ThenShouldThrowException()
        {
            var options = new HealthCheckServiceOptions();
            options.Registrations.Add(new HealthCheckRegistration("foo", new Mock<IHealthCheck>().Object, null, null));
            options.Registrations.Add(new HealthCheckRegistration("foo", new Mock<IHealthCheck>().Object, null, null));

            Assert.Throws<ArgumentException>(() => new DefaultHealthCheckService(
                new Mock<IServiceScopeFactory>().Object,
                new Mock<ILogger<DefaultHealthCheckService>>().Object,
                Microsoft.Extensions.Options.Options.Create(options)));
        }

        [Fact]
        public async Task GivenDefaultHealthCheckService_WhenAllDependenciesAreHealthy_ThenServiceShouldBeHealthy()
        {
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);

            var service1HealthCheckMock = new Mock<IHealthCheck>();
            service1HealthCheckMock.Setup(x => x.CheckHealthAsync(It.IsAny<HealthCheckContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HealthCheckResult(HealthStatus.Healthy));

            var service2HealthCheckMock = new Mock<IHealthCheck>();
            service2HealthCheckMock.Setup(x => x.CheckHealthAsync(It.IsAny<HealthCheckContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HealthCheckResult(HealthStatus.Healthy));

            var options = new HealthCheckServiceOptions();
            options.Registrations.Add(new HealthCheckRegistration("service1", service1HealthCheckMock.Object, null, null));
            options.Registrations.Add(new HealthCheckRegistration("service2", service2HealthCheckMock.Object, null, null));

            var service = new DefaultHealthCheckService(
                serviceScopeFactoryMock.Object,
                new Mock<ILogger<DefaultHealthCheckService>>().Object,
                Microsoft.Extensions.Options.Options.Create(options));

            var result = await service.CheckHealthAsync(CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(HealthStatus.Healthy, result.Status);
            Assert.Equal(2, result.Entries.Count);
            Assert.All(result.Entries, entry => Assert.Equal(HealthStatus.Healthy, entry.Value.Status));
            Assert.True(result.TotalDuration > TimeSpan.Zero);
        }

        [Fact]
        public async Task GivenDefaultHealthCheckService_WhenAtLeastOneDependencyIsUnhealthy_ThenServiceShouldBeUnhealthy()
        {
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);

            var service1HealthCheckMock = new Mock<IHealthCheck>();
            service1HealthCheckMock.Setup(x => x.CheckHealthAsync(It.IsAny<HealthCheckContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HealthCheckResult(HealthStatus.Healthy));

            var service2HealthCheckMock = new Mock<IHealthCheck>();
            service2HealthCheckMock.Setup(x => x.CheckHealthAsync(It.IsAny<HealthCheckContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HealthCheckResult(HealthStatus.Unhealthy));

            var options = new HealthCheckServiceOptions();
            options.Registrations.Add(new HealthCheckRegistration("service1", service1HealthCheckMock.Object, null, null));
            options.Registrations.Add(new HealthCheckRegistration("service2", service2HealthCheckMock.Object, null, null));

            var service = new DefaultHealthCheckService(
                serviceScopeFactoryMock.Object,
                new Mock<ILogger<DefaultHealthCheckService>>().Object,
                Microsoft.Extensions.Options.Options.Create(options));

            var result = await service.CheckHealthAsync(CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(2, result.Entries.Count);
            Assert.Equal(HealthStatus.Healthy, result.Entries.First().Value.Status);
            Assert.Equal(HealthStatus.Unhealthy, result.Entries.Last().Value.Status);
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
        }

        [Fact]
        public async Task GivenDefaultHealthCheckService_WhenHealthCheckThrowsException_ThenServiceShouldBeUnhealthy()
        {
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);

            var service1HealthCheckMock = new Mock<IHealthCheck>();
            service1HealthCheckMock.Setup(x => x.CheckHealthAsync(It.IsAny<HealthCheckContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HealthCheckResult(HealthStatus.Healthy));

            var service2HealthCheckMock = new Mock<IHealthCheck>();
            service2HealthCheckMock.Setup(x => x.CheckHealthAsync(It.IsAny<HealthCheckContext>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            var options = new HealthCheckServiceOptions();
            options.Registrations.Add(new HealthCheckRegistration("service1", service1HealthCheckMock.Object, null, null));
            options.Registrations.Add(new HealthCheckRegistration("service2", service2HealthCheckMock.Object, null, null));

            var service = new DefaultHealthCheckService(
                serviceScopeFactoryMock.Object,
                new Mock<ILogger<DefaultHealthCheckService>>().Object,
                Microsoft.Extensions.Options.Options.Create(options));

            var result = await service.CheckHealthAsync(CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(2, result.Entries.Count);
            Assert.Equal(HealthStatus.Healthy, result.Entries.First().Value.Status);
            Assert.Equal(HealthStatus.Unhealthy, result.Entries.Last().Value.Status);
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
        }
    }
}
