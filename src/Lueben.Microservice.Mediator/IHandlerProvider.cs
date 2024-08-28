using System.Collections.Generic;

namespace Lueben.Microservice.Mediator
{
    public interface IHandlerProvider
    {
        IRequestHandler<TRequest, TResponse> ProvideHandler<TRequest, TResponse>();

        IEnumerable<INotificationHandler<TNotification>> GetAllNotificationHandlers<TNotification>()
            where TNotification : INotification;
    }
}
