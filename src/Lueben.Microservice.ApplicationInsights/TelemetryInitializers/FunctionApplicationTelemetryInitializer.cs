using System.Collections.Generic;
using System.Linq;
using Lueben.ApplicationInsights;
using Lueben.Microservice.ApplicationInsights.Helpers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.ApplicationInsights.TelemetryInitializers
{
    public class FunctionApplicationTelemetryInitializer : ITelemetryInitializer
    {
        private static readonly List<string> AllScopeKeys = new()
        {
            ScopeKeys.ApplicationKey,
            ScopeKeys.ApplicationTypeKey,
            ScopeKeys.AreaKey
        };

        private readonly Dictionary<string, string> _scopePropertiesData = new();

        public FunctionApplicationTelemetryInitializer(IOptions<ApplicationLogOptions> options)
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

            var propsToRename = telemetryProps.Where(p => !p.Key.StartsWith(Constants.CompanyPrefix));
            foreach (var (prop, value) in propsToRename)
            {
                var originalPropName = FunctionPropertyHelper.GetOriginalPropertyName(prop);

                if (!AllScopeKeys.Contains(originalPropName))
                {
                    continue;
                }

                var newPropName = PropertyHelper.GetApplicationPropertyName(originalPropName);
                if (telemetryProps.TryAdd(newPropName, value))
                {
                    telemetryProps.Remove(prop);
                }
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