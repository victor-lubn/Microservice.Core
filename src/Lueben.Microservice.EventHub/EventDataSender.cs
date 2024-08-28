using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Lueben.Microservice.EventHub.Constants;
using Lueben.Microservice.EventHub.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lueben.Microservice.EventHub
{
    public class EventDataSender : IEventDataSender
    {
        private readonly EventHubProducerClient _client;
        private readonly ILogger<EventDataSender> _logger;

        public EventDataSender(EventHubProducerClient client, ILogger<EventDataSender> logger)
        {
            _client = client;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendEventAsync<T>(Event<T> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            await SendDataToEventHubAsync(new[] { data });
        }

        public async Task SendEventWithPropsAsync<T>(Event<T> eventMessage)
        {
            if (eventMessage == null)
            {
                throw new ArgumentNullException(nameof(eventMessage));
            }

            await SendDataWithPropsToEventHubAsync(new[] { eventMessage });
        }

        public async Task SendEventsListAsync<T>(IEnumerable<Event<T>> data)
        {
            await SendDataToEventHubAsync(data);
        }

        public async Task SendEventsListWithPropsAsync<T>(IEnumerable<Event<T>> data)
        {
            await SendDataWithPropsToEventHubAsync(data);
        }

        private async Task SendDataWithPropsToEventHubAsync<T>(IEnumerable<Event<T>> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            IList<EventData> events = new List<EventData>();
            foreach (var eventMessage in data)
            {
                var json = JsonConvert.SerializeObject(eventMessage.Data);
                var eventData = new EventData(Encoding.UTF8.GetBytes(json));
                eventData.Properties.AddIfNotNull(EventPropertyNames.Sender, eventMessage.Sender);
                eventData.Properties.AddIfNotNull(EventPropertyNames.Type, eventMessage.Type);
                eventData.Properties.AddIfNotNull(EventPropertyNames.Version, eventMessage.Version);
                if (eventMessage.AdditionalProperties != null && eventMessage.AdditionalProperties.Any())
                {
                    foreach (var additionalProperty in eventMessage.AdditionalProperties)
                    {
                        eventData.Properties.AddIfNotNull(additionalProperty.Key, additionalProperty.Value);
                    }
                }

                events.Add(eventData);
            }

            await SendDataToEventHubAsync(events);
        }

        private async Task SendDataToEventHubAsync<T>(IEnumerable<Event<T>> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            IList<EventData> events = new List<EventData>();
            foreach (var eventMessage in data)
            {
                var json = JsonConvert.SerializeObject(eventMessage);
                var eventData = new EventData(Encoding.UTF8.GetBytes(json));
                events.Add(eventData);
            }

            await SendDataToEventHubAsync(events);
        }

        private async Task SendDataToEventHubAsync(IList<EventData> events)
        {
            var processedEventsNumber = 0;
            var sentEventsNumber = 0;

            while (processedEventsNumber < events.Count)
            {
                try
                {
                    var eventDataBatch = events.Skip(processedEventsNumber).ToList();

                    try
                    {
                        using var eventBatch = await CreateEventBatch(_client, eventDataBatch);
                        _logger.LogDebug($"Sending {eventBatch.Count} event(s) to event hub.");
                        await _client.SendAsync(eventBatch);
                        sentEventsNumber += eventBatch.Count;
                        _logger.LogDebug($"{eventBatch.Count} event(s) successfully sent to event hub.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send events to EventHub.");
                        throw new EventCannotBeSentException(ex);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send events to EventHub.");
                    throw new EventCannotBeSentException(ex);
                }
            }

            _logger.LogInformation($"{sentEventsNumber} event(s) were successfully sent to EventHub");

            async Task<EventDataBatch> CreateEventBatch(EventHubProducerClient client, IList<EventData> eventsBatch)
            {
                _logger.LogDebug("Creating events batch.");
                var batch = await client.CreateBatchAsync();
                _logger.LogDebug("Created events batch.");

                foreach (var eventData in eventsBatch)
                {
                    if (!batch.TryAdd(eventData))
                    {
                        if (batch.Count == 0)
                        {
                            _logger.LogError("Message is too big to send to event hub. Skip to process");
                            processedEventsNumber++;
                        }

                        return batch;
                    }

                    processedEventsNumber++;
                }

                return batch;
            }
        }
    }
}