using System;
using System.Collections.Generic;
using Lueben.ApplicationInsights;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Lueben.Microservice.ApplicationInsights
{
    public class ApplicationInsightLoggerService : ILoggerService
    {
        private readonly TelemetryClient _client;

        public ApplicationInsightLoggerService(TelemetryConfiguration telemetryConfiguration)
        {
            _client = new TelemetryClient(telemetryConfiguration);
        }

        public void LogEvent(string eventName, Dictionary<string, string> props = null, Dictionary<string, double> metrics = null)
        {
            if (eventName == null)
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            var e = new EventTelemetry(eventName);

            if (props != null)
            {
                foreach (var (prop, value) in props)
                {
                    var customProp = PropertyHelper.GetCustomDataPropertyName(prop);
                    e.Properties.TryAdd(customProp, value);
                }
            }

            if (metrics != null)
            {
                foreach (var (metric, value) in metrics)
                {
                    e.Metrics.TryAdd(metric, value);
                }
            }

            _client.TrackEvent(e);
        }
    }
}