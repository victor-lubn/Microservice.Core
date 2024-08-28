using Lueben.Microservice.RestSharpClient;
using Lueben.Microservice.RestSharpClient.HealthCheck;
using Microsoft.Extensions.Logging;
using Moq;
using RestSharp;
using System.Threading.Tasks;
using Lueben.Microservice.RestSharpClient.Abstractions;
using Xunit;

namespace Lueben.Microservice.HealthChecks.Tests
{
    public class ApiHealthCheckTests
    {
        private readonly Mock<IRestSharpClient> _restClient;
        private readonly Mock<ILogger<ApiHealthCheck>> _logger;
        private readonly Mock<IRestSharpClientFactory> _restClientFactory;

        public ApiHealthCheckTests()
        {
            _restClient = new Mock<IRestSharpClient>();
            _logger = new Mock<ILogger<ApiHealthCheck>>();
            _restClientFactory = new Mock<IRestSharpClientFactory>();
        }

        [Fact]
        public async Task GivenApiHealthCheck_WhenServiceExecutesIt_ThenTheHealthyStatusShouldBeReturned()
        {
            _restClientFactory.Setup(x => x.Create(It.IsAny<string>()))
                .Returns(_restClient.Object);

            var apiHealthCheck = new ApiHealthCheck(_restClientFactory.Object, _logger.Object,"url", "scope");
            var result = await apiHealthCheck.CheckHealthAsync(null);

            Assert.Equal("Healthy", result.Status.ToString());
            Assert.Equal("Service is available.", result.Description);

            _restClient.Verify(x => x.ExecuteRequestAsync(It.IsAny<RestRequest>()), Times.Once);
        }

        [Fact]
        public async Task GivenApiHealthCheck_WhenRestClientThrowsAnException_ThenTheUnhealthyStatusShouldBeReturned()
        {
            _restClient.Setup(x => x.ExecuteRequestAsync(It.IsAny<RestRequest>()))
                .Throws(new RestClientApiException("api"));

            _restClientFactory.Setup(x => x.Create(It.IsAny<string>()))
                .Returns(_restClient.Object);

            var apiHealthCheck = new ApiHealthCheck(_restClientFactory.Object, _logger.Object, "url", "scope");

            var result = await apiHealthCheck.CheckHealthAsync(null);

            Assert.Equal("Unhealthy", result.Status.ToString());
            Assert.Equal("Service is not available.", result.Description);

            _restClient.Verify(x => x.ExecuteRequestAsync(It.IsAny<RestRequest>()), Times.Once);
        }
    }
}
