using System;
using System.Xml;

namespace Lueben.Microservice.DurableFunction
{
    public class WorkflowOptions
    {
        public string ActivityRetryInterval { get; set; }

        public string ActivityMaxRetryInterval { get; set; }

        public string ActivityTimeoutInterval { get; set; }

        public int MaxEventRetryCount { get; set; }

        public double BackoffCoefficient { get; set; }

        public TimeSpan ActivityMaxRetryIntervalTime
        {
            get
            {
                return !string.IsNullOrEmpty(ActivityMaxRetryInterval) ? XmlConvert.ToTimeSpan(ActivityMaxRetryInterval) : TimeSpan.FromDays(6);
            }
        }

        public TimeSpan ActivityRetryTimeout
        {
            get
            {
                return !string.IsNullOrEmpty(ActivityTimeoutInterval) ? XmlConvert.ToTimeSpan(ActivityTimeoutInterval) : TimeSpan.MaxValue;
            }
        }

        public TimeSpan ActivityRetryIntervalTime => XmlConvert.ToTimeSpan(ActivityRetryInterval);

        public WorkflowOptions()
        {
            ActivityRetryInterval = "PT30S";
            MaxEventRetryCount = 0; // infinite retry by default
            BackoffCoefficient = 1; // delay between attempts is the same by default
        }
    }
}
