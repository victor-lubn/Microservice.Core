using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Lueben.Microservice.RestSharpClient.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers;

namespace Lueben.Microservice.RestSharpClient
{
    public class RestSharpClient : RestClient, IRestSharpClient
    {
        private readonly ILogger<RestSharpClient> _logger;

        public RestSharpClient(HttpClient httpClient, ILogger<RestSharpClient> logger) : base(httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> ExecuteRequestAsync<T>(RestRequest request)
            where T : new()
        {
            return await ProcessRequest<T>(request);
        }

        public async Task ExecuteRequestAsync(RestRequest request)
        {
            AddCorrelationInfo(request);

            var response = await ExecuteAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessful)
            {
                LogError(request, response);

                throw new RestClientApiException(request.Resource, response.Content, response.StatusCode, response.ErrorException);
            }
        }

        [ExcludeFromCodeCoverage]
        public void SetSerializer(Func<IRestSerializer> serializerFactory)
        {
            UseSerializer(serializerFactory);
        }

        [ExcludeFromCodeCoverage]
        public void SetAuthenticator(IAuthenticator authenticator)
        {
            Authenticator = authenticator;
        }

        [ExcludeFromCodeCoverage]
        public void SetHeader(string name, string value)
        {
            this.AddDefaultHeader(name, value);
        }

        [ExcludeFromCodeCoverage]
        public void SetParameter(Parameter parameter)
        {
            AddDefaultParameter(parameter);
        }

        protected virtual async Task<T> ProcessRequest<T>(RestRequest request)
            where T : new()
        {
            AddCorrelationInfo(request);

            var response = await ExecuteAsync(request).ConfigureAwait(false);

            if (response.IsSuccessful)
            {
                return ParseContent<T>(response.Content);
            }

            LogError(request, response);

            var exception = new RestClientApiException(request.Resource, response.Content, GetResponseStatusCode(response), response.ErrorException)
            {
                ResponseData = ParseContent<T>(response.Content)
            };

            throw exception;
        }

        private static void AddCorrelationInfo(RestRequest request)
        {
            var currentActivity = Activity.Current;
            if (currentActivity?.Id != null)
            {
                request.AddHeader("Request-Id", currentActivity.Id);
            }
        }

        private void LogError(RestRequest request, RestResponse response)
        {
            var parameters = string.Join(", ", request.Parameters.Where(x => !string.IsNullOrEmpty(x.Name) && x.Name != KnownHeaders.Authorization)
                .Select(x => $"{x.Name}={x.Value}"));
            var uri = response.ResponseUri?.ToString() ?? request.Resource;

            var errorMessage = $"Request to {uri} failed with status code {response.StatusCode}; Parameters: {parameters}; Content: {response.Content}";
            _logger.LogError(errorMessage);
        }

        private T ParseContent<T>(string responseContent)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch
            {
                _logger.LogDebug("Failed to deserialize response content.");

                return default;
            }
        }

        private HttpStatusCode GetResponseStatusCode(RestResponse response)
        {
            return response.ResponseStatus switch
            {
                ResponseStatus.TimedOut => HttpStatusCode.RequestTimeout,
                ResponseStatus.Error when response.StatusCode == 0 &&
                                          response.ErrorException is HttpRequestException &&
                                          response.ErrorException?.InnerException is System.IO.IOException &&
                                          response.ErrorException?.InnerException?.InnerException is System.Net.Sockets.SocketException =>
                    HttpStatusCode.GatewayTimeout,
                _ => response.StatusCode
            };
        }
    }
}