using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Lueben.Microservice.HealthChecks.Tests
{
    public class HealthCheckFunctionTests
    {
        private readonly IServiceCollection _serviceCollection;

        public HealthCheckFunctionTests()
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());

            _serviceCollection = SetupServiceProvider();
        }

        [Fact]
        public async Task GivenHealthCheckMethod_WhenFunctionExecutesIt_ThenTheStatusShouldBeReturned()
        {
            var mockHealthServiceCheck = new Mock<HealthCheckService>();
            var healthReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), new TimeSpan(1, 0, 0, 0));

            mockHealthServiceCheck.Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(healthReport));

            var mockLogger = new Mock<ILogger>();
            var request = CreateHttpRequest();

            await using var provider = _serviceCollection.BuildServiceProvider();
            var healthCheckFunction = new HealthCheckFunction(mockHealthServiceCheck.Object);

            var response = (Models.HealthCheckResult)await healthCheckFunction.Healthcheck(request, mockLogger.Object);

            Assert.NotNull(response);
            Assert.Equal("Healthy", response.HealthCheckResponse.Status);
        }

        private static IServiceCollection SetupServiceProvider()
        {
            var serviceCollection = new ServiceCollection();

            return serviceCollection;
        }

        public static HttpRequest CreateHttpRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;

            return request;
        }
    }
}
