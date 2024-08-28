using System.Collections.Generic;

namespace Lueben.Microservice.EventHub.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddIfNotNull(this IDictionary<string, object> dictionary, string key, object value)
        {
            if (value != null)
            {
                dictionary.Add(key, value);
            }
        }
    }
}
