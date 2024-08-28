using System;
using System.Net;
using Lueben.Microservice.Api.Idempotency.Constants;
using Lueben.Microservice.Api.Idempotency.Exceptions;
using Lueben.Microservice.Api.Middleware.Middleware;
using Lueben.Microservice.Api.Models;

namespace Lueben.Microservice.Api.Idempotency.Middleware
{
    public class IdempotencyExceptionMiddleware : ExceptionHandlingMiddleware
    {
        public override ErrorResult GetErrorResult(Exception exception)
        {
            switch (exception)
            {
                case IdempotencyPayloadException _:
                {
                    return new ErrorResult(exception.Message, HttpStatusCode.UnprocessableEntity, ErrorNames.IdempotencyPayloadMismatch);
                }

                case IdempotencyConflictException _:
                {
                    return new ErrorResult(exception.Message, HttpStatusCode.Conflict, ErrorNames.IdempotencyConflict);
                }

                case IdempotencyKeyNullOrEmptyException _:
                {
                    return new ErrorResult(exception.Message, HttpStatusCode.BadRequest, ErrorNames.IdempotencyNotValid);
                }
            }

            return null;
        }
    }
}
