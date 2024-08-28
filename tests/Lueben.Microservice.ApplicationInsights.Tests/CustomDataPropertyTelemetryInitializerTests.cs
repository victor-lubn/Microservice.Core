using System.Collections.Generic;
using Lueben.ApplicationInsights;
using Lueben.Microservice.ApplicationInsights.Helpers;
using Lueben.Microservice.ApplicationInsights.TelemetryInitializers;
using Microsoft.ApplicationInsights.DataContracts;
using Xunit;

namespace Lueben.Microservice.ApplicationInsights.Tests
{
    public class CustomDataPropertyTelemetryInitializerTests
    {
        [Fact]
        public void ApplicationSettingsAdded_WhenExists()
        {
            const string mockTestProperty = "test";
            var trace = new TraceTelemetry
            {
                Properties = { { FunctionPropertyHelper.FunctionCustomPropertyPrefix + mockTestProperty, mockTestProperty } }
            };

            var initializer = new CustomDataPropertyTelemetryInitializer(new List<string> { mockTestProperty });

            initializer.Initialize(trace);

            Assert.Contains(trace.Properties, p => p.Key == PropertyHelper.GetCustomDataPropertyName(mockTestProperty));
        }

        [Fact]
        public void ApplicationSettingsNotAdded_WhenNotExists()
        {
            const string mockTestProperty = FunctionPropertyHelper.FunctionCustomPropertyPrefix + "test";
            var trace = new TraceTelemetry { Properties = { { mockTestProperty, null } } };
            
            var initializer = new CustomDataPropertyTelemetryInitializer(null);

            initializer.Initialize(trace);

            Assert.Equal(1, trace.Properties.Count);
            Assert.Contains(trace.Properties, p => p.Key == mockTestProperty);
        }
    }
}
