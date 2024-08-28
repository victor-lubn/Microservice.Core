using System.Diagnostics.CodeAnalysis;
using Lueben.Microservice.Extensions.Configuration;
using Lueben.Microservice.Json.Encrypt.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.Json.Encrypt.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJsonEncryption(this IServiceCollection services)
        {
            services.RegisterConfiguration<EncryptionOptions>(nameof(EncryptionOptions));

            return services;
        }
    }
}
