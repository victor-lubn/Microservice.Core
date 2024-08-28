using System;
using System.Xml;

namespace Lueben.Microservice.CircuitBreaker
{
    public class CircuitBreakerSettings
    {
        private const string DefaultConsistencyPriorityCheckCircuitTimeout = "PT2S";
        private const string DefaultConsistencyPriorityCheckCircuitRetryInterval = "PT0.25S";
        private const string DefaultPerformancePriorityCheckCircuitInterval = "PT2S";

        public string BreakDuration { get; set; }

        public int MaxConsecutiveFailures { get; set; }

        public string ConsistencyPriorityCheckCircuitTimeout { get; set; }

        public string ConsistencyPriorityCheckCircuitRetryInterval { get; set; }

        public string PerformancePriorityCheckCircuitInterval { get; set; }

        public TimeSpan BreakDurationTime => XmlConvert.ToTimeSpan(BreakDuration);

        public TimeSpan ConsistencyPriorityCheckCircuitTimeoutTime => GetTimeSpan(ConsistencyPriorityCheckCircuitTimeout, DefaultConsistencyPriorityCheckCircuitTimeout);

        public TimeSpan ConsistencyPriorityCheckCircuitRetryIntervalTime => GetTimeSpan(ConsistencyPriorityCheckCircuitRetryInterval, DefaultConsistencyPriorityCheckCircuitRetryInterval);

        public TimeSpan PerformancePriorityCheckCircuitIntervalTime => GetTimeSpan(PerformancePriorityCheckCircuitInterval, DefaultPerformancePriorityCheckCircuitInterval);

        public static TimeSpan GetTimeSpan(string timeSpan, string defaultTimeSpan)
        {
            return XmlConvert.ToTimeSpan(timeSpan ?? defaultTimeSpan);
        }
    }
}
