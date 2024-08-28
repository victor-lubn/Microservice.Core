using System;
using System.Collections.Generic;
using Lueben.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace Lueben.Microservice.ApplicationInsights.Extensions
{
    public static class LoggerExtensions
    {
        public static IDisposable BeginApplicationScope(this ILogger logger, ApplicationLogOptions logOptions, IDictionary<string, object> state = null)
        {
            if (logOptions == null)
            {
                throw new ArgumentNullException(nameof(logOptions));
            }

            state ??= new Dictionary<string, object>();

            AddIfNotEmpty(logOptions.ApplicationType, ScopeKeys.ApplicationTypeKey, state);
            AddIfNotEmpty(logOptions.Application, ScopeKeys.ApplicationKey, state);
            AddIfNotEmpty(logOptions.Area, ScopeKeys.AreaKey, state);

            return logger?.BeginScope(state);
        }

        private static void AddIfNotEmpty(string scopeValue, string scopeKey, IDictionary<string, object> state)
        {
            if (!string.IsNullOrEmpty(scopeValue))
            {
                state.TryAdd(scopeKey, scopeValue);
            }
        }
    }
}