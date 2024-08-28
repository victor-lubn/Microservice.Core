using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Messaging.EventHubs;
using Lueben.Microservice.EventHub.Extensions;
using Xunit;

namespace Lueben.Microservice.EventHub.Tests
{
    public class EventDataDiagnosticExtensionsTests
    {
        [Fact]
        public void GivenEventData_WhenActivityIdPropertyDoesNotExist_ThenShouldReturnFalse()
        {
            var eventData = new EventData();
            Assert.False(eventData.TryExtractId(out string id));
        }

        [Fact]
        public void GivenEventData_WhenActivityIdPropertyExist_ButItIsNotString_ThenShouldReturnFalse()
        {
            var eventData = new EventData();
            eventData.Properties.Add(EventDataDiagnosticExtensions.ActivityIdPropertyName, Guid.NewGuid());
            Assert.False(eventData.TryExtractId(out string id));
        }

        [Fact]
        public void GivenEventData_WhenActivityIdPropertyExist_ButItIsEmptyString_ThenShouldReturnFalse()
        {
            var eventData = new EventData();
            eventData.Properties.Add(EventDataDiagnosticExtensions.ActivityIdPropertyName, string.Empty);
            Assert.False(eventData.TryExtractId(out string id));
        }

        [Fact]
        public void GivenEventData_WhenActivityIdPropertyExist_ThenShouldReturnTrue()
        {
            var eventData = new EventData();
            eventData.Properties.Add(EventDataDiagnosticExtensions.ActivityIdPropertyName, Guid.NewGuid().ToString());
            Assert.True(eventData.TryExtractId(out string id));
            Assert.False(string.IsNullOrEmpty(id));
        }

        [Fact]
        public void GivenEventData_WhenCorrelationContextPropertyDoesNotExist_ThenShouldReturnFalse()
        {
            var eventData = new EventData();
            Assert.False(eventData.TryExtractContext(out IList<KeyValuePair<string, string>> context));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(1)]
        public void GivenEventData_WhenCorrelationContextPropertyInvalid_ThenShouldReturnFalse(object propertyValue)
        {
            var eventData = new EventData();
            eventData.Properties.Add(EventDataDiagnosticExtensions.CorrelationContextPropertyName, propertyValue);
            Assert.False(eventData.TryExtractContext(out IList<KeyValuePair<string, string>> context));
        }

        [Fact]
        public void GivenEventData_WhenCorrelationContextPropertyInvalid_ThenShouldReturnTrue()
        {
            var eventData = new EventData();
            eventData.Properties.Add(EventDataDiagnosticExtensions.CorrelationContextPropertyName, "foo=bar");

            Assert.True(eventData.TryExtractContext(out IList<KeyValuePair<string, string>> context));
            Assert.NotNull(context);
            Assert.Single(context);
            Assert.Equal("foo", context.First().Key);
            Assert.Equal("bar", context.First().Value);
        }

        [Fact]
        public void GivenEventData_WhenNeededDataIsSetUp_ThenShouldReturnACtivity()
        {
            var eventData = new EventData();
            var id = Guid.NewGuid().ToString();
            eventData.Properties.Add(EventDataDiagnosticExtensions.ActivityIdPropertyName, id);
            eventData.Properties.Add(EventDataDiagnosticExtensions.CorrelationContextPropertyName, "foo=bar");

            var activity = eventData.ExtractActivity("TestActivity");
            Assert.NotNull(activity);
            Assert.Equal("TestActivity", activity.DisplayName);
            Assert.Equal(id, activity.ParentId);
            Assert.Single(activity.Baggage);
            Assert.Equal("foo", activity.Baggage.First().Key);
            Assert.Equal("bar", activity.Baggage.First().Value);
        }

        [Fact]
        public void GivenEventData_WhenActivityIdIsNotFound_ThenShouldReturnActivity_ButWithoutParentIdAndBaggage()
        {
            var eventData = new EventData();
            eventData.Properties.Add(EventDataDiagnosticExtensions.CorrelationContextPropertyName, "foo=bar");

            var activity = eventData.ExtractActivity();
            Assert.NotNull(activity);
            Assert.Equal("Process", activity.DisplayName);
            Assert.Null(activity.ParentId);
            Assert.Empty(activity.Baggage);
        }

        [Fact]
        public void GivenEventData_WhenContextIsNotFound_ThenShouldReturnActivity_ButWithoutBaggage()
        {
            var eventData = new EventData();
            var id = Guid.NewGuid().ToString();
            eventData.Properties.Add(EventDataDiagnosticExtensions.ActivityIdPropertyName, id);

            var activity = eventData.ExtractActivity();
            Assert.NotNull(activity);
            Assert.Equal("Process", activity.DisplayName);
            Assert.Equal(id, activity.ParentId);
            Assert.Empty(activity.Baggage);
        }
    }
}
