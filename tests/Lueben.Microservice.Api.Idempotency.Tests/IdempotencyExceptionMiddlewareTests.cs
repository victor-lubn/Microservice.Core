using System;
using System.Net;
using Lueben.Microservice.Api.Idempotency.Constants;
using Lueben.Microservice.Api.Idempotency.Exceptions;
using Lueben.Microservice.Api.Idempotency.Middleware;
using Xunit;

namespace Lueben.Microservice.Api.Idempotency.Tests
{
    public class IdempotencyExceptionMiddlewareTests
    {
        [Theory]
        [InlineData(typeof(IdempotencyPayloadException), HttpStatusCode.UnprocessableEntity, ErrorNames.IdempotencyPayloadMismatch)]
        [InlineData(typeof(IdempotencyConflictException), HttpStatusCode.Conflict, ErrorNames.IdempotencyConflict)]
        [InlineData(typeof(IdempotencyKeyNullOrEmptyException), HttpStatusCode.BadRequest, ErrorNames.IdempotencyNotValid)]
        public void GivenGetErrorResult_WhenIsCalled_ThenExpectedResultIsReturned(Type exceptionType, HttpStatusCode statusCode, string errorName)
        {
            var middleware = new IdempotencyExceptionMiddleware();

            var exception = (Exception)Activator.CreateInstance(exceptionType);

            var errorResult = middleware.GetErrorResult(exception);

            Assert.Equal(statusCode, errorResult.StatusCode);
            Assert.Equal(errorName, errorResult.Name);
        }

        [Fact]
        public void GivenGetErrorResult_WhenIsCalledAndNoMatchedException_ThenNullIsReturned()
        {
            var middleware = new IdempotencyExceptionMiddleware();
            var exception = new Exception();

            var errorResult = middleware.GetErrorResult(exception);

            Assert.Null(errorResult);
        }
    }
}
