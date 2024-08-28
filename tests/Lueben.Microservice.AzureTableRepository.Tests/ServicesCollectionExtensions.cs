using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.AzureTableRepository.Tests
{
    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, string json)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var configuration = new ConfigurationBuilder().AddJsonStream(stream).Build();
            services.AddTransient<IConfiguration>(p => configuration);
            return services;
        }
    }
}
