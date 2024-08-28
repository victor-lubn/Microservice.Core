using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Newtonsoft.Json.Linq;

namespace Lueben.Microservice.EventHub.Validation
{
    public interface IEventValidator
    {
        Task<Event<JObject>> GetValidatedEvent(EventData eventMessage);
    }
}
