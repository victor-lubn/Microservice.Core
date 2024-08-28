using System.Collections.Generic;
using Lueben.Microservice.ApplicationInsights;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Lueben.Microservice.DurableFunction
{
    public class ReplaySafeLoggerService : ILoggerService
    {
        private readonly IDurableOrchestrationContext _context;
        private readonly ILoggerService _loggerService;

        public ReplaySafeLoggerService(ILoggerService loggerService, IDurableOrchestrationContext context)
        {
            _context = context;
            _loggerService = loggerService;
        }

        public void LogEvent(string eventName, Dictionary<string, string> props = null, Dictionary<string, double> metrics = null)
        {
            if (!_context.IsReplaying)
            {
                _loggerService.LogEvent(eventName, props, metrics);
            }
        }
    }
}
