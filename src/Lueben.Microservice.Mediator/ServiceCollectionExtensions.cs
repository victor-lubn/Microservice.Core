using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.Mediator
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediatr(this IServiceCollection services, Type handlerType, IEnumerable<Assembly> assemblies = null)
        {
            services.AddTransient<IMediator, Microservice.Mediator.Mediator>();
            services.AddTransient<IHandlerProvider, HandlerProvider>(provider => new HandlerProvider(provider));

            assemblies ??= AppDomain.CurrentDomain.GetAssemblies();

            var allHandlers = assemblies.SelectMany(s => s.GetTypes())
                .Where(handler => handler.GetInterface(handlerType.Name) != null && !handler.IsAbstract);

            foreach (var handler in allHandlers)
            {
                var interfaceType = handler.GetInterfaces().FirstOrDefault();
                services.AddTransient(interfaceType, handler);
            }

            return services;
        }
    }
}
