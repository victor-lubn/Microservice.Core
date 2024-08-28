using System.Collections.Concurrent;
using Microsoft.ApplicationInsights.Channel;

namespace Lueben.Microservice.ApplicationInsights.Tests
{
    internal class TestTelemetryChannel : ITelemetryChannel
    {
        public ConcurrentBag<ITelemetry> Telemetries = new();

        public bool? DeveloperMode { get; set; }

        public string EndpointAddress { get; set; }

        public void Dispose()
        {
        }

        public void Flush()
        {
        }

        public void Send(ITelemetry item)
        {
            Telemetries.Add(item);
        }
    }
}