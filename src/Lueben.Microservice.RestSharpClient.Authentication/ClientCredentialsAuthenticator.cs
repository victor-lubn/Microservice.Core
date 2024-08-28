using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;

namespace Lueben.Microservice.RestSharpClient.Authentication
{
    public class ClientCredentialsAuthenticator : IAuthenticator
    {
        private readonly IServiceApiAuthorizer _authorizer;
        private readonly string[] _scopes;

        public ClientCredentialsAuthenticator(IServiceApiAuthorizer authorizer, string[] scopes)
        {
            _authorizer = authorizer;
            _scopes = scopes;
        }

        public async ValueTask Authenticate(RestClient client, RestRequest request)
        {
            var token = await _authorizer.GetAccessTokenAsync(_scopes);
            request.AddOrUpdateHeader(KnownHeaders.Authorization, $"Bearer {token}");
        }
    }
}
