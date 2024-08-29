using System;
using System.Runtime.Serialization;

namespace Lueben.Microservice.DurableFunction.Exceptions
{
    [Serializable]
    public class EventDataProcessFailureException : Exception
    {
        public EventDataProcessFailureException()
        {
        }

        public EventDataProcessFailureException(string message) : base(message)
        {
        }

        public EventDataProcessFailureException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}