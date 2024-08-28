using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.ApplicationInsights.TelemetryOptions
{
    [ExcludeFromCodeCoverage]
    public class ApplicationInsightsTelemetryOptions
    {
        private readonly IServiceCollection _services;

        internal ApplicationInsightsTelemetryOptions(IServiceCollection services)
        {
            _services = services;
        }

        public ApplicationInsightsTelemetryOptions AddTelemetryInitializer<TInitializer>()
            where TInitializer : class, ITelemetryInitializer
        {
            _services.AddSingleton<ITelemetryInitializer, TInitializer>();
            return this;
        }

        public ApplicationInsightsTelemetryOptions AddTelemetryInitializer<TInitializer>(Func<IServiceProvider, TInitializer> factory)
            where TInitializer : class, ITelemetryInitializer
        {
            _services.AddSingleton<ITelemetryInitializer, TInitializer>(factory);
            return this;
        }
    }
}
