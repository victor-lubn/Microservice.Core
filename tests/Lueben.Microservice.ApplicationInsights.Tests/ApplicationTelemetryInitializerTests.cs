using Lueben.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;
using Xunit;

namespace Lueben.Microservice.ApplicationInsights.Tests
{
    public class ApplicationTelemetryInitializerTests
    {
        [Fact]
        public void GivenApplicationTelemetryEnabled_WhenLogOptionsAreSet_ThenTelemetryContainsValuesFromSettings()
        {
            var trace = new TraceTelemetry("message");
            var options = Options.Create(new ApplicationLogOptions
            {
                ApplicationType = "TestType",
                Application = "TestApp",
                Area = "TestArea"
            });

            var initializer = new ApplicationTelemetryInitializer(options);

            initializer.Initialize(trace);

            Assert.Equal(3, trace.Properties.Count);
            Assert.Contains(trace.Properties, p => p.Key == PropertyHelper.GetApplicationPropertyName(ScopeKeys.ApplicationTypeKey) && p.Value == options.Value.ApplicationType);
            Assert.Contains(trace.Properties, p => p.Key == PropertyHelper.GetApplicationPropertyName(ScopeKeys.ApplicationKey) && p.Value == options.Value.Application);
            Assert.Contains(trace.Properties, p => p.Key == PropertyHelper.GetApplicationPropertyName(ScopeKeys.AreaKey) && p.Value == options.Value.Area);
        }

        [Fact]
        public void GivenApplicationTelemetryEnabled_WhenLogOptionsArePartiallySet_ThenTelemetryContainsOnlySetValues()
        {
            var trace = new TraceTelemetry("message");
            var options = Options.Create(new ApplicationLogOptions
            {
                ApplicationType = "TestType"
            });

            var initializer = new ApplicationTelemetryInitializer(options);

            initializer.Initialize(trace);

            Assert.Equal(1, trace.Properties.Count);
            Assert.Contains(trace.Properties, p => p.Value == options.Value.ApplicationType);
            Assert.Contains(trace.Properties, p => p.Key == PropertyHelper.GetApplicationPropertyName(ScopeKeys.ApplicationTypeKey));
        }

        [Fact]
        public void GivenApplicationTelemetryEnabled_WhenPropertyIsAlreadySet_ThenItIsNotOverriden()
        {
            var scopeValue = "scope";
            var settingsValue = "settings";
            var options = Options.Create(new ApplicationLogOptions
            {
                ApplicationType = settingsValue,
            });

            var trace = new TraceTelemetry("message")
            {
                Properties = { { PropertyHelper.GetApplicationPropertyName(ScopeKeys.ApplicationTypeKey), scopeValue } }
            };
            var initializer = new ApplicationTelemetryInitializer(options);

            initializer.Initialize(trace);

            Assert.Equal(1, trace.Properties.Count);
            Assert.Contains(trace.Properties, p => p.Value == scopeValue);
        }
    }
}
