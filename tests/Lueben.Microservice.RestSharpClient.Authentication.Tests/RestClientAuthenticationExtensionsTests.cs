using Lueben.Microservice.RestSharpClient.Abstractions;
using Moq;
using RestSharp.Authenticators;

namespace Lueben.Microservice.RestSharpClient.Authentication.Tests
{
    public class RestClientAuthenticationExtensionsTests
    {
        private readonly Mock<IRestSharpClient> _clientMock;

        public RestClientAuthenticationExtensionsTests()
        {
            _clientMock = new Mock<IRestSharpClient>();
        }

        [Fact]
        public void GivenAddFunctionKeyAuthentication_WhenCalledWithFunctionKey_ThenFunctionKeyAuthenticatorIsAdded()
        {
            var functionKey = "test function key";

            _clientMock.Object.AddFunctionKeyAuthentication(functionKey);

            _clientMock.Verify(x => x.SetAuthenticator(It.IsAny<FunctionKeyAuthenticator>()), Times.Once);
        }

        [Fact]
        public void GivenAddFunctionKeyAuthentication_WhenCalledWithotFunctionKey_ThenFunctionKeyAuthenticatorIsNotAdded()
        {
            var functionKey = string.Empty;

            _clientMock.Object.AddFunctionKeyAuthentication(functionKey);

            _clientMock.Verify(x => x.SetAuthenticator(It.IsAny<FunctionKeyAuthenticator>()), Times.Never);
        }

        [Fact]
        public void GivenAddClientCredentialsAuthentication_WhenCalled_ThenClientCredentialsAuthenticatorIsAdded()
        {
            var scopes = new []{ "test scope" };
            Func<string[], string> tokenResolver = (input) => "test token";
            var authenticatorMock = new Mock<IServiceApiAuthorizer>();
            authenticatorMock.Setup(x => x.GetAccessTokenAsync(It.IsAny<IReadOnlyCollection<string>>()))
                .ReturnsAsync("Token");
            _clientMock.Object.AddClientCredentialsAuthentication(authenticatorMock.Object, scopes);

            _clientMock.Verify(x => x.SetAuthenticator(It.IsAny<ClientCredentialsAuthenticator>()), Times.Once);
        }

        [Fact]
        public void GivenAddOAuthAuthentication_WhenCalledWithToken_ThenJwtAuthenticatorIsAdded()
        {
            var token = "test token";
            _clientMock.Object.AddOAuthAuthentication(token);

            _clientMock.Verify(x => x.SetAuthenticator(It.IsAny<JwtAuthenticator>()), Times.Once);
        }

        [Fact]
        public void GivenAddOAuthAuthentication_WhenCalledWithNoToken_ThenJwtAuthenticatorIsNotAdded()
        {
            var token = string.Empty;
            _clientMock.Object.AddOAuthAuthentication(token);

            _clientMock.Verify(x => x.SetAuthenticator(It.IsAny<JwtAuthenticator>()), Times.Never);
        }
    }
}