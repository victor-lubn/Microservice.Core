using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lueben.Microservice.Api.Models;
using Lueben.Microservice.Diagnostics;
using Lueben.Microservice.Notification.Constants;
using Lueben.Microservice.Notification.Options;
using Lueben.Microservice.RestSharpClient;
using Lueben.Microservice.RestSharpClient.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;

namespace Lueben.Microservice.Notification
{
    public abstract class ApiClient
    {
        private readonly ILogger<ApiClient> _logger;
        private readonly IRestSharpClient _restClient;
        private readonly ApiClientOptions _apiClientOptions;

        protected ApiClient(IRestSharpClientFactory restSharpClientFactory,
            IOptionsSnapshot<ApiClientOptions> apiClientOptions,
            ILogger<ApiClient> logger)
        {
            Ensure.ArgumentNotNull(restSharpClientFactory, nameof(restSharpClientFactory));
            Ensure.ArgumentNotNull(apiClientOptions, nameof(apiClientOptions));
            Ensure.ArgumentNotNull(apiClientOptions.Value, nameof(apiClientOptions));
            Ensure.ArgumentNotNullOrEmpty(apiClientOptions.Value.BaseUrl, nameof(apiClientOptions.Value.BaseUrl));

            _logger = logger;
            _restClient = restSharpClientFactory.Create(this);
            _apiClientOptions = apiClientOptions.Value;
        }

        protected async Task<T> Get<T>(string url, IEnumerable<KeyValuePair<string, string>> headers = null, object queryParameters = null, params object[] parameters)
            where T : class, new()
        {
            return await GetData<T>(url, headers, queryParameters, parameters);
        }

        protected async Task<T> Post<T>(string url, object body, IEnumerable<KeyValuePair<string, string>> headers = null, object queryParameters = null, params object[] parameters)
            where T : new()
        {
            try
            {
                var request = InitializeRequest(url, Method.Post, parameters, headers);
                request.PopulateQueryStringParameters(queryParameters);
                request.AddHeader(Headers.IdempotencyKey, Guid.NewGuid().ToString());
                request.PopulateBody(body);
                return await _restClient.ExecuteRequestAsync<T>(request);
            }
            catch (RestClientApiException ex)
            {
                ex.ResponseData = DeserializeErrorResponseData(ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while posting object to API ({url})");
                throw;
            }
        }

        private async Task<T> GetData<T>(string url, IEnumerable<KeyValuePair<string, string>> headers = null, object queryParameters = null, params object[] parameters)
            where T : class, new()
        {
            try
            {
                var request = InitializeRequest(url, Method.Get, parameters, headers);
                request.PopulateQueryStringParameters(queryParameters);
                var response = await _restClient.ExecuteRequestAsync<T>(request);
                return response;
            }
            catch (RestClientApiException ex)
            {
                ex.ResponseData = DeserializeErrorResponseData(ex);
                throw;
            }
            catch (Exception ex)
            {
                var message = $"Error while get object from API ({url})";
                _logger.LogError(ex, message);
                throw;
            }
        }

        private RestRequest InitializeRequest(string url,
            Method method,
            IReadOnlyList<object> parameters = null,
            IEnumerable<KeyValuePair<string, string>> headers = null)
        {
            var uriService = new Uri(url);
            var request = new RestRequest(uriService.AbsoluteUri, method);
            request.PopulateUrlSegmentParameters(url, parameters);
            request.PopulateHeaders(headers);
            request.AddHeader(Headers.LuebenRequestConsumer, _apiClientOptions.LuebenRequestConsumer);
            return request;
        }

        private ErrorResult DeserializeErrorResponseData(RestClientApiException ex)
        {
            if (ex.ResponseContent != null)
            {
                try
                {
                    var responseData = JsonConvert.DeserializeObject<ErrorResult>(ex.ResponseContent);
                    return responseData;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to deserialize error response content.");
                }
            }

            return null;
        }
    }
}