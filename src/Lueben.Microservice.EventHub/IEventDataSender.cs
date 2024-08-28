using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lueben.Microservice.EventHub
{
    public interface IEventDataSender
    {
        Task SendEventAsync<T>(Event<T> data);

        Task SendEventsListAsync<T>(IEnumerable<Event<T>> data);

        Task SendEventWithPropsAsync<T>(Event<T> eventMessage);

        Task SendEventsListWithPropsAsync<T>(IEnumerable<Event<T>> data);
    }
}