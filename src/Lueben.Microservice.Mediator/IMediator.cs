using System.Threading.Tasks;

namespace Lueben.Microservice.Mediator
{
    public interface IMediator
    {
        Task<TResponse> Send<TRequest, TResponse>(TRequest request);

        Task Publish<TNotification>(TNotification notification)
            where TNotification : INotification;
    }
}
