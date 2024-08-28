using Lueben.Microservice.RestSharpClient.Abstractions;

namespace Lueben.Microservice.RestSharpClient.Authentication
{
    public static class ApimRestClientExtensions
    {
        public const string ApiVersionHeader = "apiversion";

        public static IRestSharpClient AddApimHeaders(this IRestSharpClient client, ApimClientOptions options, IServiceApiAuthorizer authorizer)
        {
            if (!string.IsNullOrEmpty(options.Scope))
            {
                client.AddClientCredentialsAuthentication(authorizer, new[] { options.Scope });
            }

            if (!string.IsNullOrEmpty(options.ApiVersion))
            {
                client.SetHeader(ApiVersionHeader, options.ApiVersion);
            }

            return client;
        }
    }
}