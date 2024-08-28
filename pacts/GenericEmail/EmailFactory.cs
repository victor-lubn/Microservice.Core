using System.Net.Http;
using Lueben.Microservice.RestSharpClient;
using Lueben.Microservice.RestSharpClient.Abstractions;
using Lueben.Microservice.Serialization;
using Microsoft.Extensions.Logging;

namespace Lueben.Microservice.GenericEmail.Tests.Pact
{
    public class EmailFactory : RestSharpClientFactory
    {
        public EmailFactory(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory) : base(loggerFactory, httpClientFactory)
        {
        }

        public override IRestSharpClient Create(object client)
        {
            var restClient = base.Create(client);
            restClient.SetSerializerSettings(FunctionJsonSerializerSettingsProvider.CreateSerializerSettings());

            return restClient;
        }
    }
}