using System;
using System.Collections.Generic;
using System.Linq;
using Lueben.ApplicationInsights;
using Lueben.Microservice.ApplicationInsights.Extensions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Xunit;

namespace Lueben.Microservice.ApplicationInsights.Tests
{
    public class ApplicationInsightLoggerServiceTests : IDisposable
    {
        private readonly TestTelemetryChannel _channel = new();
        private readonly TelemetryConfiguration _telemetryConfiguration;
        private bool _disposed;

        public ApplicationInsightLoggerServiceTests()
        {
            _telemetryConfiguration = new TelemetryConfiguration { TelemetryChannel = _channel };
        }

        [Fact]
        public void GivenApplicationLogSettingsAreSet_WhenEventIsCreated_ThenEventNameContainsApplicationLogSettingsValues()
        {
            const string mockEventName = "test";
            ILoggerService service = new ApplicationInsightLoggerService(_telemetryConfiguration);

            service.LogEvent(mockEventName);

            var telemetry = _channel.Telemetries.SingleOrDefault();
            Assert.NotNull(telemetry);
            Assert.IsType<EventTelemetry>(telemetry);
            Assert.Equal(mockEventName, (telemetry as EventTelemetry)?.Name);
        }

        [Fact]
        public void GivenApplicationLogSettingsAreSet_WhenEventIsCreatedWithCompanyPrefix_ThenEventNameIsNotChanged()
        {
            var eventName = Constants.CompanyPrefix + Constants.Separator + "test";
            ILoggerService service = new ApplicationInsightLoggerService(_telemetryConfiguration);

            service.LogEvent(eventName);

            var telemetry = (EventTelemetry)_channel.Telemetries.SingleOrDefault();
            Assert.Equal(eventName, telemetry?.Name);
        }

        [Fact]
        public void GivenApplicationLogSettingsAreSet_WhenEventNameNotSpecified_ThenArgumentNullExceptionIsThrown()
        {
            ILoggerService service = new ApplicationInsightLoggerService(_telemetryConfiguration);

            Assert.Throws<ArgumentNullException>(() => service.LogEvent(null));
        }

        [Fact]
        public void GivenApplicationLogSettingsAreSet_WhenLogRetryEvent_ThenCorrespondingPropertyIsAdded()
        {
            var eventName = Constants.CompanyPrefix + Constants.Separator + "test";
            ILoggerService service = new ApplicationInsightLoggerService(_telemetryConfiguration);
            var retryCount = 2;

            service.LogRetryEvent(eventName, retryCount);

            var telemetry = (EventTelemetry)_channel.Telemetries.SingleOrDefault();
            Assert.True(telemetry?.Properties.Values.Contains(retryCount.ToString()));
        }

        [Fact]
        public void GivenApplicationLogSettingsAreSet_WhenLogRetryEventWithoutRetries_ThenEventIsNotAdded()
        {
            var eventName = Constants.CompanyPrefix + Constants.Separator + "test";
            ILoggerService service = new ApplicationInsightLoggerService(_telemetryConfiguration);
            var retryCount = 0;

            service.LogRetryEvent(eventName, retryCount);

            var telemetry = (EventTelemetry)_channel.Telemetries.SingleOrDefault();
            
            Assert.Null(telemetry);
        }

        [Fact]
        public void GivenApplicationLogSettingsAreSet_WhenPropsArePassed_ThenTheyAreAddedToEvent()
        {
            var eventName = Constants.CompanyPrefix + Constants.Separator + "test";
            ILoggerService service = new ApplicationInsightLoggerService(_telemetryConfiguration);
            var propName = "testProp";
            var propValue = "testPropValue";
            var props = new Dictionary<string, string>
            {
                { propName, propValue}
            };

            service.LogEvent(eventName, props);

            var telemetry = (EventTelemetry)_channel.Telemetries.SingleOrDefault();
            Assert.True(telemetry?.Properties.Values.Contains(propValue));
        }

        [Fact]
        public void GivenApplicationLogSettingsAreSet_WhenMetricsArePassed_ThenTheyAreAddedToEvent()
        {
            var eventName = Constants.CompanyPrefix + Constants.Separator + "test";
            ILoggerService service = new ApplicationInsightLoggerService(_telemetryConfiguration);
            var metricName = "testProp";
            var metricValue = 10;
            var metrics = new Dictionary<string, double>
            {
                { metricName, metricValue}
            };

            service.LogEvent(eventName, null, metrics);

            var telemetry = (EventTelemetry)_channel.Telemetries.SingleOrDefault();
            Assert.True(telemetry?.Metrics.Values.Contains(metricValue));
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
                _telemetryConfiguration.Dispose();
            }

            _disposed = true;
        }
    }
}
