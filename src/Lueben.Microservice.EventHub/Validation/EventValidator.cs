using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Lueben.Microservice.EventHub.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lueben.Microservice.EventHub.Validation
{
    public abstract class EventValidator : IEventValidator
    {
        private readonly ILogger<EventValidator> _logger;

        protected EventValidator(ILogger<EventValidator> logger)
        {
            _logger = logger;
        }

        public async Task<Event<JObject>> GetValidatedEvent(EventData eventMessage)
        {
            var eventData = await GetValidatedData(eventMessage);

            return IsNotValidEvent(eventData) ? default : eventData;
        }

        protected abstract bool EventHandlerExists(string eventType);

        private bool IsNotValidEvent(Event<JObject> eventData)
        {
            if (eventData == null)
            {
                _logger.LogWarning("EventIncorrectModel");
                return true;
            }

            if (!EventHandlerExists(eventData.Type))
            {
                _logger.LogWarning("EventIncorrectType");
                return true;
            }

            return false;
        }

        private Task<Event<JObject>> GetValidatedData(EventData eventMessage)
        {
            try
            {
                Event<JObject> eventData;
                var message = Encoding.UTF8.GetString(eventMessage.Body.ToArray());
                var eventType = eventMessage.GetEventType();
                if (eventType == null)
                {
                    eventData = JsonConvert.DeserializeObject<Event<JObject>>(message);
                }
                else
                {
                    eventData = new Event<JObject>
                    {
                        Type = eventType,
                        Data = JsonConvert.DeserializeObject<JObject>(message)
                    };
                }

                if (string.IsNullOrEmpty(eventData?.Type))
                {
                    _logger.LogError("Event type is not defined.");
                }
                else if (eventData.Data == null)
                {
                    _logger.LogError("Event data is not defined.");
                }
                else
                {
                    return Task.FromResult(eventData);
                }
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "Failed to deserialize event.");
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, "Failed to deserialize event.");
            }

            return Task.FromResult((Event<JObject>)null);
        }
    }
}
