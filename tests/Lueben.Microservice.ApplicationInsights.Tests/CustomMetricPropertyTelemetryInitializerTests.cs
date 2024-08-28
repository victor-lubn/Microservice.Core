using Lueben.ApplicationInsights;
using Lueben.Microservice.ApplicationInsights.Helpers;
using Lueben.Microservice.ApplicationInsights.TelemetryInitializers;
using Microsoft.ApplicationInsights.DataContracts;
using System.Collections.Generic;
using Xunit;

namespace Lueben.Microservice.ApplicationInsights.Tests
{
    public class CustomMetricPropertyTelemetryInitializerTests
    {
        [Fact]
        public void ApplicationSettingsAdded_WhenExists()
        {
            const string mockTestMetric = "test";
            var trace = new EventTelemetry
            {
                Metrics = { { FunctionPropertyHelper.FunctionCustomPropertyPrefix + mockTestMetric, 1.0 } }
            };

            var initializer = new CustomDataMetricTelemetryInitializer(new List<string> { mockTestMetric });

            initializer.Initialize(trace);

            Assert.Contains(trace.Metrics, p => p.Key == PropertyHelper.GetCustomDataPropertyName(mockTestMetric));
        }

        [Fact]
        public void ApplicationSettingsNotAdded_WhenNotExists()
        {
            const string mockTestMetric = FunctionPropertyHelper.FunctionCustomPropertyPrefix + "test";
            var trace = new EventTelemetry { Metrics = { { mockTestMetric, 1.0 } } };

            var initializer = new CustomDataMetricTelemetryInitializer(null);

            initializer.Initialize(trace);

            Assert.Equal(1, trace.Metrics.Count);
            Assert.Contains(trace.Metrics, p => p.Key == mockTestMetric);
        }
    }
}
