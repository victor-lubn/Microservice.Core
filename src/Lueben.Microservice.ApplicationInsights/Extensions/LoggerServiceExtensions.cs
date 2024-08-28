using System.Collections.Generic;

namespace Lueben.Microservice.ApplicationInsights.Extensions
{
    public static class LoggerServiceExtensions
    {
        public const string RetryCountKey = "RetryCount";

        public static void LogRetryEvent(this ILoggerService loggerService, string eventName, int retryCount)
        {
            if (retryCount == 0)
            {
                return;
            }

            loggerService.LogEvent(eventName, new Dictionary<string, string>
            {
                [RetryCountKey] = retryCount.ToString()
            });
        }
    }
}
