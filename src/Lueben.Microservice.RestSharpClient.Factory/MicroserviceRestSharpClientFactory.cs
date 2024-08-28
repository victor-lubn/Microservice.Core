using System;
using System.Net;
using System.Net.Http;
using Lueben.Microservice.CircuitBreaker;
using Lueben.Microservice.Interceptors;
using Lueben.Microservice.RestSharpClient.Abstractions;
using Lueben.Microservice.RestSharpClient.Authentication;
using Lueben.Microservice.RetryPolicy;
using Lueben.Microservice.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.RestSharpClient.Factory
{
    public sealed class MicroserviceRestSharpClientFactory : RestSharpClientFactory
    {
        private readonly IOptionsSnapshot<RestSharpClientOptions> _options;
        private readonly IServiceApiAuthorizer _authorizer;
        private readonly ICircuitBreakerClient _circuitBreakerClient;
        private readonly IRetryPolicy<RestClientApiException> _retryPolicy;

        public MicroserviceRestSharpClientFactory(ILoggerFactory loggerFactory,
            IOptionsSnapshot<RestSharpClientOptions> options,
            IServiceApiAuthorizer authorizer,
            ICircuitBreakerClient circuitBreakerClient,
            IRetryPolicy<RestClientApiException> retryPolicy,
            IHttpClientFactory httpClientFactory) : base(loggerFactory, httpClientFactory)
        {
            _options = options;
            _authorizer = authorizer;
            _circuitBreakerClient = circuitBreakerClient;
            _retryPolicy = retryPolicy;
        }

        public override IRestSharpClient Create(object client)
        {
            var restSharpClient = base.Create(client);
            var clientName = client.GetType().Name;

            return CreateClient(restSharpClient, clientName);
        }

        public override IRestSharpClient Create(string sectionName)
        {
            var restSharpClient = base.Create(sectionName);

            return CreateClient(restSharpClient, sectionName);
        }

        private static bool CheckStatusCode(HttpStatusCode httpStatusCode)
        {
            return (int)httpStatusCode >= 500 ||
                   httpStatusCode == HttpStatusCode.TooManyRequests ||
                   httpStatusCode == HttpStatusCode.RequestTimeout;
        }

        private IRestSharpClient CreateClient(IRestSharpClient restSharpClient, string clientName)
        {
            var clientOptions = _options.Get(clientName);

            if (clientOptions == null)
            {
                throw new Exception($"{nameof(RestSharpClientOptions)} not set for {clientName}.");
            }

            AddHeaders(clientOptions, restSharpClient);
            restSharpClient = AddInterceptors(clientOptions, restSharpClient);

            restSharpClient.SetSerializerSettings(FunctionJsonSerializerSettingsProvider.CreateSerializerSettings());

            return restSharpClient;
        }

        private void AddHeaders(RestSharpClientOptions clientOptions, IRestSharpClient restClient)
        {
            if (!string.IsNullOrEmpty(clientOptions.FunctionKey))
            {
                restClient.AddFunctionKeyAuthentication(clientOptions.FunctionKey);
            }
            else if (!string.IsNullOrEmpty(clientOptions.Scope))
            {
                var apimOptions = new ApimClientOptions
                {
                    ApiVersion = clientOptions.ApiVersion,
                    Scope = clientOptions.Scope
                };
                restClient.AddApimHeaders(apimOptions, _authorizer);
            }
        }

        private IRestSharpClient AddInterceptors(RestSharpClientOptions clientOptions, IRestSharpClient restSharpClient)
        {
            if (clientOptions.EnableRetry)
            {
                restSharpClient = restSharpClient.AddRetryPolicy(_retryPolicy, ex =>
                {
                    if (ex.StatusCode.HasValue && CheckStatusCode(ex.StatusCode.Value))
                    {
                        throw new RetriableOperationFailedException();
                    }
                });
            }

            if (!string.IsNullOrEmpty(clientOptions.CircuitBreakerId))
            {
                restSharpClient = restSharpClient.AddCircuitBreaker(_circuitBreakerClient, clientOptions.CircuitBreakerId);
            }

            return restSharpClient;
        }
    }
}