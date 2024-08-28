using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text;

namespace Lueben.Microservice.Extensions.Configuration.Tests
{
    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, string json)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var configuration = new ConfigurationBuilder().AddJsonStream(stream).Build();
            services.AddTransient<IConfiguration>(_ => configuration);
            return services;
        }
    }
}
