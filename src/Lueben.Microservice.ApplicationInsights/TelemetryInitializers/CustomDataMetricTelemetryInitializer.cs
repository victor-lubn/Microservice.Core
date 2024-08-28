using System.Collections.Generic;
using Lueben.Microservice.ApplicationInsights.Helpers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Lueben.Microservice.ApplicationInsights.TelemetryInitializers
{
    public class CustomDataMetricTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IList<string> _customMetrics;

        public CustomDataMetricTelemetryInitializer(IList<string> customMetrics)
        {
            _customMetrics = customMetrics;
        }

        public void Initialize(ITelemetry telemetry)
        {
            CustomEventHelper.RenameEvents((telemetry as ISupportMetrics)?.Metrics, _customMetrics);
        }
    }
}
