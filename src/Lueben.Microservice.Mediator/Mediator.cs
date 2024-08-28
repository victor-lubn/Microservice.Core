using System.Threading.Tasks;

namespace Lueben.Microservice.Mediator
{
    public class Mediator : IMediator
    {
        private readonly IHandlerProvider _handlerProvider;

        public Mediator(IHandlerProvider handlerProvider)
        {
            _handlerProvider = handlerProvider;
        }

        public async Task<TResponse> Send<TRequest, TResponse>(TRequest request)
        {
            var handler = _handlerProvider.ProvideHandler<TRequest, TResponse>();

            return await handler.Handle(request);
        }

        public async Task Publish<TNotification>(TNotification notification)
            where TNotification : INotification
        {
            foreach (var handler in _handlerProvider.GetAllNotificationHandlers<TNotification>())
            {
                await handler.Handle(notification);
            }
        }
    }
}
