using Azure.Messaging.EventHubs;
using Lueben.Microservice.EventHub.Constants;

namespace Lueben.Microservice.EventHub.Extensions
{
    public static class EventDataExtensions
    {
        public static string GetEventType(this EventData eventData)
        {
            return eventData.Properties.TryGetValue(EventPropertyNames.Type, out var eventType) ? eventType.ToString() : null;
        }

        public static string GetEventSender(this EventData eventData)
        {
            return eventData.Properties.TryGetValue(EventPropertyNames.Sender, out var eventSender) ? eventSender.ToString() : null;
        }

        public static int? GetEventVersion(this EventData eventData)
        {
            return eventData.Properties.TryGetValue(EventPropertyNames.Version, out var versionValue) && int.TryParse(versionValue.ToString(), out var version) ? version : null;
        }
    }
}
