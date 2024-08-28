using System;
using Lueben.Microservice.Api.Idempotency.Constants;

namespace Lueben.Microservice.Api.Idempotency.Exceptions
{
    public class IdempotencyConflictException : Exception
    {
        public IdempotencyConflictException()
            : base(ErrorMessages.IdempotencyConflictError)
        {
        }
    }
}
