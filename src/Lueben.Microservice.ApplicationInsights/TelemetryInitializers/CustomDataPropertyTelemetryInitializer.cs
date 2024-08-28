using System.Collections.Generic;
using Lueben.Microservice.ApplicationInsights.Helpers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Lueben.Microservice.ApplicationInsights.TelemetryInitializers
{
    public class CustomDataPropertyTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IList<string> _customProperties;

        public CustomDataPropertyTelemetryInitializer(IList<string> customProperties)
        {
            _customProperties = customProperties;
        }

        public void Initialize(ITelemetry telemetry)
        {
            CustomEventHelper.RenameEvents((telemetry as ISupportProperties)?.Properties, _customProperties);
        }
    }
}