using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.Mediator
{
    public class HandlerProvider : IHandlerProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public HandlerProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IRequestHandler<TRequest, TResponse> ProvideHandler<TRequest, TResponse>()
        {
            return (IRequestHandler<TRequest, TResponse>)_serviceProvider.GetService(typeof(IRequestHandler<TRequest, TResponse>));
        }

        public IEnumerable<INotificationHandler<TNotification>> GetAllNotificationHandlers<TNotification>()
            where TNotification : INotification
        {
            var handlers = _serviceProvider.GetServices(typeof(INotificationHandler<TNotification>));
            return (IEnumerable<INotificationHandler<TNotification>>)handlers;
        }
    }
}
