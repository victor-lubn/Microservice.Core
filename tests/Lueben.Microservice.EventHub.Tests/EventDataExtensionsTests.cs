using Azure.Messaging.EventHubs;
using Lueben.Microservice.EventHub.Constants;
using Lueben.Microservice.EventHub.Extensions;
using Xunit;

namespace Lueben.Microservice.EventHub.Tests
{
    public class EventDataExtensionsTests
    {
        [Fact]
        public void GivenGetEventType_WhenTypePropertyDoesNotExist_ThenShouldReturnNull()
        {
            var eventData = new EventData();
            Assert.Null(eventData.GetEventType());
        }

        [Fact]
        public void GivenGetEventType_WhenTypePropertyExists_ThenShouldReturnItsValue()
        {
            var type = "test type";
            var eventData = new EventData();
            eventData.Properties.Add(EventPropertyNames.Type, type);
            Assert.Equal(type, eventData.GetEventType());
        }

        [Fact]
        public void GivenGetEventSender_WhenSenderPropertyDoesNotExist_ThenShouldReturnNull()
        {
            var eventData = new EventData();
            Assert.Null(eventData.GetEventSender());
        }

        [Fact]
        public void GivenGetEventSender_WhenSenderPropertyExists_ThenShouldReturnItsValue()
        {
            var sender = "sender";
            var eventData = new EventData();
            eventData.Properties.Add(EventPropertyNames.Sender, sender);
            Assert.Equal(sender, eventData.GetEventSender());
        }

        [Fact]
        public void GivenGetEventVersion_WhenVersionPropertyDoesNotExist_ThenShouldReturnNull()
        {
            var eventData = new EventData();
            Assert.Null(eventData.GetEventVersion());
        }

        [Fact]
        public void GivenGetEventVersion_WhenVersionPropertyExists_ThenShouldReturnItsValue()
        {
            var version = 1;
            var eventData = new EventData();
            eventData.Properties.Add(EventPropertyNames.Version, version);
            Assert.Equal(version, eventData.GetEventVersion());
        }


        [Fact]
        public void GivenGetEventVersion_WhenVersionNotNumber_ThenShouldReturnNull()
        {
            var version = "wrong data";
            var eventData = new EventData();
            eventData.Properties.Add(EventPropertyNames.Version, version);
            Assert.Null(eventData.GetEventVersion());
        }
    }
}
