using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;

namespace Lueben.ApplicationInsights.Exceptions
{
    public class ExceptionLowLevelTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IDictionary<SeverityLevel, IEnumerable<string>> _exceptionNamesByLevel;

        public ExceptionLowLevelTelemetryInitializer(IOptions<ExceptionsLowLevelLogOptions> options)
        {
            _exceptionNamesByLevel = options.Value.Exceptions;
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (_exceptionNamesByLevel == null
                || !_exceptionNamesByLevel.Any()
                || !(telemetry is ExceptionTelemetry exceptionTelemetry))
            {
                return;
            }

            var exceptionTypeAndBaseTypesFullNames = GetExceptionTypes(exceptionTelemetry.Exception)
                .Select(s => s.FullName)
                .Distinct()
                .ToList();

            var levelAndExceptions = _exceptionNamesByLevel
                .FirstOrDefault(x => x.Value
                    .Any(s => !string.IsNullOrEmpty(s) && exceptionTypeAndBaseTypesFullNames
                        .Any(t => t.EndsWith(s))));

            if (!IsDefault(levelAndExceptions) && exceptionTelemetry.SeverityLevel > levelAndExceptions.Key)
            {
                exceptionTelemetry.SeverityLevel = levelAndExceptions.Key;
            }
        }

        private static bool IsDefault<T>(T value)
            where T : struct
        {
            var isDefault = value.Equals(default(T));

            return isDefault;
        }

        private static IEnumerable<Type> GetExceptionTypes(Exception exception)
        {
            var exceptionTypes = GetTypeAndBaseTypes(exception.GetType()).ToList();

            var currentInnerException = exception.InnerException;
            while (currentInnerException != null)
            {
                var types = GetTypeAndBaseTypes(currentInnerException.GetType());
                exceptionTypes.AddRange(types);
                currentInnerException = currentInnerException.InnerException;
            }

            return exceptionTypes;
        }

        private static IEnumerable<Type> GetTypeAndBaseTypes(Type type)
        {
            yield return type;

            var currentBaseType = type.BaseType;
            while (currentBaseType != null)
            {
                yield return currentBaseType;
                currentBaseType = currentBaseType.BaseType;
            }
        }
    }
}
