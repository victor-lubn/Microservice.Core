using RestSharp;

namespace Lueben.Microservice.RestSharpClient.Authentication.Tests
{
    public class FunctionKeyAuthenticatorTests
    {
        [Fact]
        public void GivenFunctionKeyAuthenticator_WhenIsUsed_ThenAddsFunctionKeyHeader()
        {
            const string key = "key";
            var authenticator = new FunctionKeyAuthenticator(key);

            var request = new RestRequest();
            authenticator.Authenticate(new RestClient(), request);

            var header = request.Parameters.FirstOrDefault(p => p.Name == "x-functions-key" && p.Type == ParameterType.HttpHeader);
            Assert.NotNull(header);
            Assert.Equal(key, header?.Value);
        }
    }
}
