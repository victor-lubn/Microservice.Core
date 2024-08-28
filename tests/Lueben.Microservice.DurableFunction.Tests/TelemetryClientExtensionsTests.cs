using Lueben.Microservice.DurableFunction.Extensions;
using Microsoft.ApplicationInsights;
using System;
using Xunit;
using DurableTask.Core;
using Microsoft.ApplicationInsights.Extensibility;

namespace Lueben.Microservice.DurableFunction.Tests
{
    public class TelemetryClientExtensionsTests
    {
        [Fact]
        public void GivenTrackUsingCorrelationTraceContext_WhenTelemetryClientIsNotPassed_ThenArgumentNullExceptionIsThrown()
        {
            TelemetryClient telemetryClient = null;
            
            Assert.Throws<ArgumentNullException>(() => telemetryClient.TrackUsingCorrelationTraceContext());
        }

        [Fact]
        public void GivenTrackUsingCorrelationTraceContext_WhenCorrelationContextIsNull_ThenExceptionIsThrown()
        {
            var telemetryClient = GetTelemetryClient();

            Assert.Throws<Exception>(() => telemetryClient.TrackUsingCorrelationTraceContext());
        }

        [Fact]
        public void GivenTrackUsingCorrelationTraceContext_WhenCorrelationContextIsNotW3C_ThenInvalidOperationExceptionIsThrown()
        {
            var telemetryClient = GetTelemetryClient();
            CorrelationTraceContext.Current = new NullObjectTraceContext();

            Assert.Throws<InvalidOperationException>(() => telemetryClient.TrackUsingCorrelationTraceContext());
        }

        [Fact]
        public void GivenTrackUsingCorrelationTraceContext_WhenCorrelationContextIsW3C_ThenTraceIsTracked()
        {
            var telemetryClient = GetTelemetryClient();
            CorrelationTraceContext.Current = new W3CTraceContext();

            telemetryClient.TrackUsingCorrelationTraceContext();
        }

        private TelemetryClient GetTelemetryClient()
        {
            var configuration = new TelemetryConfiguration();
            configuration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());

            return new TelemetryClient(configuration);
        }
    }
}
