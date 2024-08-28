using System;
using Lueben.Microservice.RestSharpClient.Abstractions;
using RestSharp.Authenticators;

namespace Lueben.Microservice.RestSharpClient.Authentication
{
    public static class RestClientAuthenticationExtensions
    {
        public static IRestSharpClient AddFunctionKeyAuthentication(this IRestSharpClient client, string functionKey)
        {
            if (!string.IsNullOrEmpty(functionKey))
            {
                client.SetAuthenticator(new FunctionKeyAuthenticator(functionKey));
            }

            return client;
        }

        public static IRestSharpClient AddClientCredentialsAuthentication(this IRestSharpClient client, IServiceApiAuthorizer authorizer, string[] scopes)
        {
            client.SetAuthenticator(new ClientCredentialsAuthenticator(authorizer, scopes));

            return client;
        }

        public static IRestSharpClient AddOAuthAuthentication(this IRestSharpClient client, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                client.SetAuthenticator(new JwtAuthenticator(token));
            }

            return client;
        }
    }
}