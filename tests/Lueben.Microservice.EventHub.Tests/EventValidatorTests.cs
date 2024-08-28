using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Azure.Messaging.EventHubs;
using Lueben.Microservice.EventHub.Constants;
using Lueben.Microservice.EventHub.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Lueben.Microservice.EventHub.Tests
{
    public class EventValidatorTests
    {
        private readonly EventValidator _validator;

        public EventValidatorTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _validator = fixture.Create<EventValidator>();
        }

        [Fact]
        public async Task GivenGetValidatedEvent_WhenEventMessageIsIncorrectJson_ThenShouldReturnNull()
        {
            var eventMessage = new Event<JObject>
            {
                Type = "test"
            };
            var message = JsonConvert.SerializeObject(eventMessage);
            var invalidMessage = message + "test string";
            var eventData = new EventData(invalidMessage);

            var result = await _validator.GetValidatedEvent(eventData);

            Assert.Null(result);
        }

        [Fact]
        public async Task GivenGetValidatedEvent_WhenEventDataIsNotDefined_ThenShouldReturnNull()
        {
            var eventMessage = new Event<JObject>
            {
                Type = "test"
            };

            var message = JsonConvert.SerializeObject(eventMessage);
            var eventData = new EventData(message);

            var result = await _validator.GetValidatedEvent(eventData);

            Assert.Null(result);
        }

        [Fact]
        public async Task GivenGetValidatedEvent_WhenEventTypeIsNotDefined_ThenShouldReturnNull()
        {
            var eventMessage = new Event<JObject>();

            var message = JsonConvert.SerializeObject(eventMessage);
            var eventData = new EventData(message);

            var result = await _validator.GetValidatedEvent(eventData);

            Assert.Null(result);
        }

        [Fact]
        public async Task GivenGetValidatedEvent_WhenEventTypeFromPropertiesIsNotDefined_ThenShouldReturnNull()
        {
            var eventMessage = new Event<JObject>();

            var message = JsonConvert.SerializeObject(eventMessage);
            var eventData = new EventData(message);
            eventData.Properties.Add(EventPropertyNames.Type, "");

            var result = await _validator.GetValidatedEvent(eventData);

            Assert.Null(result);
        }

        [Fact]
        public async Task GivenGetValidatedEvent_WhenUnexpectedEventType_ThenShouldReturnNull()
        {
            var eventMessage = new Event<JObject>
            {
                Type = "test",
                Data = new JObject()
            };

            var message = JsonConvert.SerializeObject(eventMessage);
            var eventData = new EventData(message);

            var result = await _validator.GetValidatedEvent(eventData);

            Assert.Null(result);
        }
    }
}
