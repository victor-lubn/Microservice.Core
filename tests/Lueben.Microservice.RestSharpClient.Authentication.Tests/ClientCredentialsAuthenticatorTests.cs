using Moq;
using RestSharp;

namespace Lueben.Microservice.RestSharpClient.Authentication.Tests
{
    public class ClientCredentialsAuthenticatorTests
    {
        [Fact]

        public async Task GivenAuthenticator_ShouldGetBearerToken_AndSetItToAuthorizationHeaderOfRequest()
        {
            var authenticatorMock = new Mock<IServiceApiAuthorizer>();
            authenticatorMock.Setup(x => x.GetAccessTokenAsync(It.IsAny<IReadOnlyCollection<string>>()))
                .ReturnsAsync("Token");
            var authenticator = new ClientCredentialsAuthenticator(authenticatorMock.Object, new[] { "foo" });
            var client = new RestClient();
            var request = new RestRequest();
            
            await authenticator.Authenticate(client, request);
            
            var headers = request.Parameters.Where(p => p.Type == ParameterType.HttpHeader);
            var authHeader = headers.First(h => h.Name == KnownHeaders.Authorization);
            
            Assert.NotNull(authHeader);
            Assert.Equal("Bearer Token", authHeader.Value);
        }
    }
}
