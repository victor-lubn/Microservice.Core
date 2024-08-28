using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Lueben.Microservice.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

namespace Lueben.Microservice.FileLogging
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFileLogging(this IServiceCollection services)
        {
            services.RegisterConfiguration<FileLogOptions>(nameof(FileLogOptions));

            var provider = services.BuildServiceProvider();
            var options = provider.GetService<IOptions<FileLogOptions>>();

            var fileLogName = $"{Assembly.GetCallingAssembly().GetName().Name}.log";
            var homePath = Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.Process);
            string fileLogPath;

            if (string.IsNullOrEmpty(homePath))
            {
                fileLogPath = options.Value.LogFilePath ?? $"C:\\Logs\\{fileLogName}";
            }
            else
            {
                fileLogPath = Path.Combine(homePath, options.Value.DefaultLogFilesFolder, fileLogName);
            }

            var fileSizeLimitBytes = options.Value.LogFileSizeLimitBytes > 0 ? options.Value.LogFileSizeLimitBytes : 10485760;

            if (!Enum.TryParse(options.Value.LogLevel, out LogLevel logLevel))
            {
                logLevel = LogLevel.Error;
            }

            var logEventLevel = (LogEventLevel)Enum.ToObject(typeof(LogEventLevel), (int)logLevel);

            var logger = new LoggerConfiguration()
                .WriteTo.File(fileLogPath,
                    logEventLevel,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: fileSizeLimitBytes)
                .CreateLogger();

            return services
                .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));
        }
    }
}