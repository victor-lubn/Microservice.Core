using System;
using Lueben.ApplicationInsights;
using Lueben.Microservice.ApplicationInsights.TelemetryOptions;
using Lueben.Microservice.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.ApplicationInsights.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationInsightsTelemetry(this IServiceCollection services, Action<ApplicationInsightsTelemetryOptions> action = null)
        {
            var telemetryOptions = new ApplicationInsightsTelemetryOptions(services);
            telemetryOptions.AddTelemetryInitializer<ApplicationTelemetryInitializer>();

            action?.Invoke(telemetryOptions);

            return services.RegisterConfiguration<ApplicationLogOptions>(nameof(ApplicationLogOptions))
                .AddSingleton<ILoggerService, ApplicationInsightLoggerService>();
        }
    }
}
