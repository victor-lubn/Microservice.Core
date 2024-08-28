using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Azure.Core;
using Azure.Identity;
using Lueben.Microservice.Options.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace Lueben.Microservice.Options.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        private const string Delimiter = ":";
        private const string RefreshKeyName = "Sentinel";
        private const uint DefaultCacheExpirationSeconds = 300;

        public static IConfigurationBuilder AddLuebenAzureAppConfiguration(this IConfigurationBuilder configurationBuilder, string applicationConfigurationPrefix = null, string globalConfigurationPrefix = null)
        {
            var token = new ManagedIdentityCredential();
            return configurationBuilder.AddLuebenAzureAppConfiguration(token, applicationConfigurationPrefix, globalConfigurationPrefix);
        }

        public static IConfigurationBuilder AddLuebenAzureAppConfiguration(this IConfigurationBuilder configurationBuilder, TokenCredential credential, string applicationConfigurationPrefix = null, string globalConfigurationPrefix = null)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings:AzureAppConfiguration");
            var endpoint = Environment.GetEnvironmentVariable("AzureAppConfigurationEndpoint");

            Func<AzureAppConfigurationOptions, AzureAppConfigurationOptions> connectFunc;
            if (!string.IsNullOrEmpty(connectionString))
            {
                connectFunc = options => options.Connect(connectionString);
            }
            else if (!string.IsNullOrEmpty(endpoint))
            {
                connectFunc = options => options.Connect(new Uri(endpoint), credential);
            }
            else
            {
                return configurationBuilder;
            }

            applicationConfigurationPrefix ??= Environment.GetEnvironmentVariable("ApplicationConfigurationPrefix");
            globalConfigurationPrefix ??= Environment.GetEnvironmentVariable("GlobalConfigurationPrefix");
            var sentinelKey = GetKey(globalConfigurationPrefix, RefreshKeyName);

            var builder = new ConfigurationBuilder();
            if (!string.IsNullOrEmpty(globalConfigurationPrefix))
            {
                builder.AddAzureAppConfiguration(options => connectFunc(options).ConfigureOptions(new AppConfigurationOptions
                {
                    SentinelKey = sentinelKey,
                    TokenCredential = credential,
                    Prefix = globalConfigurationPrefix,
                    UseFeatureFlags = false
                }));
            }

            builder.AddAzureAppConfiguration(options => connectFunc(options).ConfigureOptions(new AppConfigurationOptions
            {
                SentinelKey = sentinelKey,
                TokenCredential = credential,
                Prefix = applicationConfigurationPrefix ?? string.Empty,
                UseFeatureFlags = true
            }));

            return configurationBuilder.AddAzureAppConfigurationAfterEnvironment(builder.Sources);
        }

        public static IConfigurationBuilder AddAzureAppConfigurationAfterEnvironment(this IConfigurationBuilder configurationBuilder, IList<IConfigurationSource> sources)
        {
            var environmentSettingsIndex = configurationBuilder.Sources.IndexOf(configurationBuilder.Sources.OfType<EnvironmentVariablesConfigurationSource>().FirstOrDefault());

            environmentSettingsIndex = environmentSettingsIndex == -1 ? 0 : environmentSettingsIndex;

            foreach (var source in sources)
            {
                configurationBuilder.Sources.Insert(environmentSettingsIndex++, source);
            }

            return configurationBuilder;
        }

        [ExcludeFromCodeCoverage]
        public static void ConfigureOptions(this AzureAppConfigurationOptions options, AppConfigurationOptions configurationOptions)
        {
            var filter = GetKey(configurationOptions.Prefix, KeyFilter.Any);
            options.Select(filter);
            if (!string.IsNullOrEmpty(configurationOptions.Prefix))
            {
                options.TrimKeyPrefix($"{configurationOptions.Prefix}{Delimiter}");
            }

            options.ConfigureKeyVault(kv => kv.SetCredential(configurationOptions.TokenCredential));

            if (configurationOptions.UseFeatureFlags)
            {
                options.UseFeatureFlags();
            }

            if (configurationOptions.AutoRefresh)
            {
                if (!uint.TryParse(Environment.GetEnvironmentVariable("Configuration:CacheExpiration"), out var cacheExpiration))
                {
                    cacheExpiration = DefaultCacheExpirationSeconds;
                }

                options.ConfigureRefresh(refreshOptions =>
                {
                    var key = configurationOptions.SentinelKey;
                    refreshOptions.Register(key, refreshAll: true).SetCacheExpiration(TimeSpan.FromSeconds(cacheExpiration));
                });
            }
        }

        public static string GetKey(string prefix, string name)
        {
            return string.IsNullOrEmpty(prefix) ? name : $"{prefix}{Delimiter}{name}";
        }
    }
}