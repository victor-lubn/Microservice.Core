using System;
using System.Net.Http;
using Lueben.Microservice.RestSharpClient.Abstractions;
using Microsoft.Extensions.Logging;

namespace Lueben.Microservice.RestSharpClient
{
    public class RestSharpClientFactory : IRestSharpClientFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpClientFactory _httpClientFactory;

        public RestSharpClientFactory(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public virtual IRestSharpClient Create(object client)
        {
            return CreateClient(client.GetType().Name);
        }

        public virtual IRestSharpClient Create(string sectionName)
        {
            return CreateClient(sectionName);
        }

        private IRestSharpClient CreateClient(string clientName)
        {
            var logger = _loggerFactory.CreateLogger<RestSharpClient>();
            var client = _httpClientFactory.CreateClient(clientName);
            var restClient = new RestSharpClient(client, logger);

            return restClient;
        }
    }
}