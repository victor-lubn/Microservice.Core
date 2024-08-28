using Azure.Core;

namespace Lueben.Microservice.Options.Options
{
    public class AppConfigurationOptions
    {
        public string SentinelKey { get; set; }

        public string Prefix { get; set; }

        public TokenCredential TokenCredential { get; set; }

        public bool UseFeatureFlags { get; set; }

        public bool AutoRefresh { get; set; } = true;
    }
}
