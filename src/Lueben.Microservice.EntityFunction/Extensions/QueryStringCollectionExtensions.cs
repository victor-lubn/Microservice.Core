using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace Lueben.Microservice.EntityFunction.Extensions
{
    public static class QueryStringCollectionExtensions
    {
        public static T ToObject<T>(this IDictionary<string, string> source)
            where T : new()
        {
            var json = JsonConvert.SerializeObject(source);

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T ToQueryObject<T>(this HttpRequestData request)
            where T : new()
        {
            var queryParams = request.Query.ToDictionary();

            var json = JsonConvert.SerializeObject(queryParams);

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static IDictionary<string, string> ToDictionary(this NameValueCollection source)
        {
            if (source == null)
            {
                return new Dictionary<string, string>();
            }

            return source.AllKeys.ToDictionary(t => t, t => source[t]);
        }
    }
}
