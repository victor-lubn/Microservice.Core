using Lueben.Microservice.RestSharpClient.Abstractions;
using Newtonsoft.Json;
using RestSharp.Serializers.NewtonsoftJson;

namespace Lueben.Microservice.RestSharpClient
{
    public static class RestClientExtensions
    {
        public static void SetSerializerSettings(this IRestSharpClient client, JsonSerializerSettings settings)
        {
            client.SetSerializer(() => new JsonNetSerializer(settings));
        }
    }
}