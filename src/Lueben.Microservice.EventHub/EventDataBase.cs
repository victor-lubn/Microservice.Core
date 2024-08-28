using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.EventHub
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public abstract class EventDataBase<T>
        where T : class
    {
        private readonly string _eventType;

        protected abstract T Data { get; }

        protected EventDataBase(string eventType)
        {
            _eventType = eventType;
        }

        public Event<T> CreateEvent(string sender = null, int version = 1, IDictionary<string, object> additionalProperties = null) => new()
        {
            Type = _eventType,
            Data = Data,
            Version = version,
            Sender = sender ?? Environment.ExpandEnvironmentVariables("%WEBSITE_SITE_NAME%"),
            AdditionalProperties = additionalProperties
        };
    }
}
