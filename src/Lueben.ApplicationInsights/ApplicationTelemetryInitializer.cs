using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;

namespace Lueben.ApplicationInsights
{
    public class ApplicationTelemetryInitializer : ITelemetryInitializer
    {
        private readonly Dictionary<string, string> _scopePropertiesData = new();

        public ApplicationTelemetryInitializer(IOptions<ApplicationLogOptions> options)
        {
            AddIfNotEmpty(_scopePropertiesData, ScopeKeys.ApplicationTypeKey, options.Value.ApplicationType);
            AddIfNotEmpty(_scopePropertiesData, ScopeKeys.ApplicationKey, options.Value.Application);
            AddIfNotEmpty(_scopePropertiesData, ScopeKeys.AreaKey, options.Value.Area);
        }

        public void Initialize(ITelemetry telemetry)
        {
            var telemetryProps = (telemetry as ISupportProperties)?.Properties;
            if (telemetryProps == null)
            {
                return;
            }

            foreach (var (prop, value) in _scopePropertiesData)
            {
                telemetryProps.TryAdd(prop, value);
            }
        }

        private static void AddIfNotEmpty(IDictionary<string, string> props, string prop, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            prop = PropertyHelper.GetApplicationPropertyName(prop);
            props.TryAdd(prop, value);
        }
    }
}