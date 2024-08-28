using System;
using System.Diagnostics.CodeAnalysis;

namespace Lueben.Microservice.CircuitBreaker
{
    [ExcludeFromCodeCoverage]
    public class RetriableOperationFailedException : Exception
    {
        private const string DefaultErrorMessage =
            "Retriable operation failed several times. Circuit breaker needs to be moved to Open state.";

        public RetriableOperationFailedException()
            : base(DefaultErrorMessage)
        {
        }

        public RetriableOperationFailedException(Exception innerException)
            : base(DefaultErrorMessage, innerException)
        {
        }
    }
}
