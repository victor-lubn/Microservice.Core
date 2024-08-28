using System;
using Lueben.Microservice.Api.Idempotency.Constants;

namespace Lueben.Microservice.Api.Idempotency.Exceptions
{
    public class IdempotencyKeyNullOrEmptyException : Exception
    {
        public IdempotencyKeyNullOrEmptyException()
            : base(ErrorMessages.IdempotencyKeyNullOrEmptyError)
        {
        }
    }
}
