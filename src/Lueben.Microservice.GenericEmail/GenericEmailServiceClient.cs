using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Lueben.Microservice.Diagnostics;
using Lueben.Microservice.GenericEmail.Models.Requests;
using Lueben.Microservice.GenericEmail.Models.Responses;
using Lueben.Microservice.RestSharpClient.Abstractions;
using Microsoft.Extensions.Options;
using RestSharp;

[assembly: InternalsVisibleTo("Lueben.Microservice.GenericEmail.Tests")]

namespace Lueben.Microservice.GenericEmail
{
    public class GenericEmailServiceClient : IGenericEmailServiceClient
    {
        public const string GenericEmailServiceApiUrl = "/SendEmail";
        public const string GenericDynamicEmailServiceApiUrl = "/SendDynamicEmail";
        public const string GenericOnBehalfEmailServiceApiUrl = "/SendEmailOnBehalfOfService";
        public const string GenericEmailServiceVersionUrl = "/GetVersion";

        private readonly GenericEmailServiceOptions _genericEmailServiceOptions;
        private readonly IRestSharpClient _restClient;

        public GenericEmailServiceClient(IRestSharpClientFactory restSharpClientFactory, IOptionsSnapshot<GenericEmailServiceOptions> genericEmailServiceOptions)
        {
            Ensure.ArgumentNotNull(restSharpClientFactory, nameof(restSharpClientFactory));
            Ensure.ArgumentNotNull(genericEmailServiceOptions, nameof(genericEmailServiceOptions));
            Ensure.ArgumentNotNull(genericEmailServiceOptions.Value, nameof(genericEmailServiceOptions));
            Ensure.ArgumentNotNullOrEmpty(genericEmailServiceOptions.Value.GenericEmailServiceApiBaseUrl, nameof(genericEmailServiceOptions.Value.GenericEmailServiceApiBaseUrl));

            _genericEmailServiceOptions = genericEmailServiceOptions.Value;
            _restClient = restSharpClientFactory.Create(this);
        }

        public async Task<ApiResponse> SendEmail(GenericEmailRequest data)
        {
            Ensure.ArgumentNotNull(data, nameof(data));

            var resourceUri = GetApiUrl(GenericEmailServiceApiUrl);
            var request = new RestRequest(resourceUri, Method.Post);
            request.AddJsonBody(data);
            return await _restClient.ExecuteRequestAsync<ApiResponse>(request);
        }

        public async Task<ApiResponse> SendDynamicEmail(DynamicEmailRequest data)
        {
            Ensure.ArgumentNotNull(data, nameof(data));

            var resourceUri = GetApiUrl(GenericDynamicEmailServiceApiUrl);
            var request = new RestRequest(resourceUri, Method.Post);
            request.AddJsonBody(data);

            return await _restClient.ExecuteRequestAsync<ApiResponse>(request);
        }

        public async Task SendOnBehalfOfServiceEmail(GenericEmailRequest data)
        {
            Ensure.ArgumentNotNull(data, nameof(data));

            var resourceUri = GetApiUrl(GenericOnBehalfEmailServiceApiUrl);
            var request = new RestRequest(resourceUri, Method.Post);
            request.AddJsonBody(data);
            await _restClient.ExecuteRequestAsync(request);
        }

        public async Task<string> GetVersion()
        {
            var resourceUri = GetApiUrl(GenericEmailServiceVersionUrl);
            var request = new RestRequest(resourceUri);
            var result = await _restClient.ExecuteRequestAsync<object>(request);
            return result?.ToString();
        }

        internal Uri GetApiUrl(string apiUri)
        {
            Ensure.ArgumentNotNullOrEmpty(apiUri, nameof(apiUri));

            return new Uri(_genericEmailServiceOptions.GenericEmailServiceApiBaseUrl + apiUri);
        }
    }
}