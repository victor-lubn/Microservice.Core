using System.Threading.Tasks;

namespace Lueben.Microservice.Mediator
{
    public interface INotificationHandler<in TNotification>
        where TNotification : INotification
    {
        Task Handle(TNotification notification);
    }
}
