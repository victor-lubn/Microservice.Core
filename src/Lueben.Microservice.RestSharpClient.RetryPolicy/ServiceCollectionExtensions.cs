using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Lueben.Microservice.RetryPolicy;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.RestSharpClient.RetryPolicy
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static readonly List<HttpStatusCode> RestClientRetryStatusCodes = new List<HttpStatusCode>
        {
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.Conflict,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout
        };

        public static IServiceCollection AddRestClientRetryPolicy(this IServiceCollection services, ICollection<HttpStatusCode> retryStatusCodes = null)
        {
            retryStatusCodes ??= RestClientRetryStatusCodes;
            return services.AddRetryPolicy<RestClientApiException>(e => retryStatusCodes.Contains(e.StatusCode.GetValueOrDefault()));
        }
    }
}