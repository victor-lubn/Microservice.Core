using System;
using Lueben.Microservice.Api.Idempotency.Constants;

namespace Lueben.Microservice.Api.Idempotency.Exceptions
{
    public class IdempotencyPayloadException : Exception
    {
        public IdempotencyPayloadException()
            : base(ErrorMessages.ChangedPayloadError)
        {
        }
    }
}
