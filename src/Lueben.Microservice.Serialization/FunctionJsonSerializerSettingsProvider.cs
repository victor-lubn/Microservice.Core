using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.Serialization
{
    public static class FunctionJsonSerializerSettingsProvider
    {
        private static readonly DefaultContractResolver SharedContractResolver = new()
        {
            NamingStrategy = CreateNamingStrategy()
        };

        public static NamingStrategy CreateNamingStrategy() => new CamelCaseNamingStrategy();

        public static JsonSerializerSettings CreateSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = SharedContractResolver
            };

            settings.Converters.Add(new StringEnumConverter
            {
                NamingStrategy = CreateNamingStrategy(),
                AllowIntegerValues = false
            });

            return settings;
        }
    }
}