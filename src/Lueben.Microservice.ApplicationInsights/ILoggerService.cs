using System.Collections.Generic;

namespace Lueben.Microservice.ApplicationInsights
{
    public interface ILoggerService
    {
        void LogEvent(string eventName, Dictionary<string, string> props = null, Dictionary<string, double> metrics = null);
    }
}