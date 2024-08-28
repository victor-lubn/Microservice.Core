using Lueben.Microservice.RestSharpClient.Abstractions;
using Moq;

namespace Lueben.Microservice.RestSharpClient.Authentication.Tests
{
    public class ApimRestClientExtensionsTests
    {
        [Fact]
        public void GivenAddApimHeaders_WhenCalledWithScope_ThenClientCredentialsAuthenticationIsAdded()
        {
            var authorizerMock = new Mock<IServiceApiAuthorizer>();
            var options = new ApimClientOptions
            {
                Scope = "test scope"
            };

            var clientMock = new Mock<IRestSharpClient>();

            clientMock.Object.AddApimHeaders(options, authorizerMock.Object);

            clientMock.Verify(x => x.SetAuthenticator(It.IsAny<ClientCredentialsAuthenticator>()), Times.Once);
            clientMock.Verify(x => x.SetHeader(ApimRestClientExtensions.ApiVersionHeader, options.ApiVersion), Times.Never);
        }

        [Fact]
        public void GivenAddApimHeaders_WhenCalledWithVersion_ThenVersionHeaderIsAdded()
        {
            var authorizerMock = new Mock<IServiceApiAuthorizer>();
            var options = new ApimClientOptions
            {
                ApiVersion = "v1"
            };

            var clientMock = new Mock<IRestSharpClient>();

            clientMock.Object.AddApimHeaders(options, authorizerMock.Object);

            clientMock.Verify(x => x.SetAuthenticator(It.IsAny<ClientCredentialsAuthenticator>()), Times.Never);
            clientMock.Verify(x => x.SetHeader(ApimRestClientExtensions.ApiVersionHeader, options.ApiVersion), Times.Once);
        }
    }
}
