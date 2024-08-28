using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Lueben.Microservice.CircuitBreaker
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class CircuitBreakerOpenStateException : Exception
    {
        public CircuitBreakerOpenStateException() : base("Circuit breaker is in Open state.")
        {
        }

        protected CircuitBreakerOpenStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}