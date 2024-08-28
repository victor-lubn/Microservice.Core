using System;
using System.Collections.Generic;
using System.Linq;
using Lueben.ApplicationInsights;
using Lueben.Microservice.ApplicationInsights.Extensions;
using Lueben.Microservice.ApplicationInsights.TelemetryInitializers;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using Xunit;
using ScopeKeys = Lueben.ApplicationInsights.ScopeKeys;

namespace Lueben.Microservice.ApplicationInsights.Tests
{
    public class FunctionApplicationLoggingScopeTests : IDisposable
    {
        private readonly TestTelemetryChannel _channel = new();
        private readonly ILogger _logger;
        private readonly ILoggerService _loggerService;
        private readonly IOptions<ApplicationLogOptions> _options;
        private bool _disposed;

        public FunctionApplicationLoggingScopeTests()
        {
            var options = Options.Create(new ApplicationLogOptions
            {
                ApplicationType = "TestType",
                Application = "TestApp",
                Area = "TestArea"
            });
            _options = options;

            var telemetryConfiguration = new TelemetryConfiguration { TelemetryChannel = _channel };
            telemetryConfiguration.TelemetryInitializers.Add(new FunctionApplicationTelemetryInitializer(_options));
            var logProvider = new ApplicationInsightsLoggerProvider(Options.Create(telemetryConfiguration), Options.Create(new ApplicationInsightsLoggerOptions()));
            logProvider.SetScopeProvider(new LoggerExternalScopeProvider());
            _loggerService = new ApplicationInsightLoggerService(telemetryConfiguration);
            _logger = logProvider.CreateLogger("test category");
        }

        [Fact]
        public void GivenTelemetryInitializersAreSet_WhenLogOptionsNotPassed_ThenArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => _logger.BeginApplicationScope(null));
        }

        [Fact]
        public void GivenTelemetryInitializersAreSet_WhenApplicationScopeKeyIsOverridenInInnerScope_ThenTelemetryHasOverridenValue()
        {
            const string mockNewArea = "newArea";

            using (_logger.BeginApplicationScope(_options.Value))
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    [ScopeKeys.AreaKey] = mockNewArea
                }))
                {
                    _logger.Log(LogLevel.Information, "test message");
                }
            }

            var telemetry = _channel.Telemetries.SingleOrDefault();
            var trace = Assert.IsType<TraceTelemetry>(telemetry);
            Assert.Contains(trace.Properties, p => p.Key == PropertyHelper.GetApplicationPropertyName(ScopeKeys.AreaKey) && p.Value == mockNewArea);
        }

        [Fact]
        public void GivenTelemetryInitializersAreSet_WhenApplicationScopeIsSet_ThenEventTelemetryHasApplicationCustomProperties()
        {
            using (_logger.BeginApplicationScope(_options.Value))
            {
                _loggerService.LogEvent("test");
            }

            var telemetry = _channel.Telemetries.SingleOrDefault();
            var eventTelemetry = Assert.IsType<EventTelemetry>(telemetry);
            Assert.Contains(eventTelemetry.Properties, p => p.Key == PropertyHelper.GetApplicationPropertyName(ScopeKeys.ApplicationTypeKey) && p.Value == _options.Value.ApplicationType);
            Assert.Contains(eventTelemetry.Properties, p => p.Key == PropertyHelper.GetApplicationPropertyName(ScopeKeys.ApplicationKey) && p.Value == _options.Value.Application);
            Assert.Contains(eventTelemetry.Properties, p => p.Key == PropertyHelper.GetApplicationPropertyName(ScopeKeys.AreaKey) && p.Value == _options.Value.Area);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _channel.Dispose();
            }

            _disposed = true;
        }
    }
}