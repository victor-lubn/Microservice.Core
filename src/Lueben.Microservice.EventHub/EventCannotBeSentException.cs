using System;
using System.Diagnostics.CodeAnalysis;

namespace Lueben.Microservice.EventHub
{
    [ExcludeFromCodeCoverage]
    public class EventCannotBeSentException : Exception
    {
        private const string EventCannotBeSentError = "Event cannot be sent to Event Hub.";

        public EventCannotBeSentException() : base(message: EventCannotBeSentError)
        {
        }

        public EventCannotBeSentException(Exception innerException) : base(message: EventCannotBeSentError,
            innerException)
        {
        }
    }
}
