using System;
using DurableTask.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Lueben.Microservice.DurableFunction.Extensions
{
    public static class TelemetryClientExtensions
    {
        public static void TrackUsingCorrelationTraceContext(
            this TelemetryClient telemetryClient)
        {
            if (telemetryClient == null)
            {
                throw new ArgumentNullException(nameof(telemetryClient));
            }

            if (CorrelationTraceContext.Current == null)
            {
                throw new Exception("Current CorrelationTraceContext is not set.");
            }

            if (!(CorrelationTraceContext.Current is W3CTraceContext correlationContext))
            {
                throw new InvalidOperationException($"Expecting a correlation trace context of {nameof(W3CTraceContext)}, but the context is of type {CorrelationTraceContext.Current.GetType()}");
            }

            var trace = new TraceTelemetry($"Activity Id: {correlationContext.TraceParent} ParentSpanId: {correlationContext.ParentSpanId}");
            trace.Context.Operation.Id = correlationContext.TelemetryContextOperationId;
            trace.Context.Operation.ParentId = correlationContext.TelemetryContextOperationParentId;
            telemetryClient.TrackTrace(trace);
        }
    }
}