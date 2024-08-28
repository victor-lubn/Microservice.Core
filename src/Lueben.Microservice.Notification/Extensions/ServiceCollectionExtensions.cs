using Lueben.Microservice.Extensions.Configuration;
using Lueben.Microservice.Notification.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.Notification.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotificationServiceClient(this IServiceCollection services)
        {
            return services
                .RegisterConfiguration<NotificationClientOptions>(nameof(NotificationClientOptions))
                .AddTransient<INotificationServiceClient, NotificationServiceClient>();
        }
    }
}
