using System;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers;

namespace Lueben.Microservice.RestSharpClient.Abstractions
{
    public interface IRestSharpClient
    {
        Task<T> ExecuteRequestAsync<T>(RestRequest request)
            where T : new();

        Task ExecuteRequestAsync(RestRequest request);

        void SetSerializer(Func<IRestSerializer> serializerFactory);

        void SetAuthenticator(IAuthenticator authenticator);

        void SetHeader(string name, string value);

        void SetParameter(Parameter parameter);
    }
}