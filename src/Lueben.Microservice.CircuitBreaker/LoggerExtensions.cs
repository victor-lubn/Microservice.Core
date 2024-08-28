using Microsoft.Extensions.Logging;

namespace Lueben.Microservice.CircuitBreaker
{
    public static class LoggerExtensions
    {
        private const LogLevel DefaultLogLevel = LogLevel.Debug;

        public static void LogCircuitBreakerMessage(this ILogger logger, string circuitBreakerId, string message)
        {
            logger.Log(DefaultLogLevel, message);
        }
    }
}
