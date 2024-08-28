using System.Net;
using Lueben.Microservice.Api.Middleware.Constants;
using Lueben.Microservice.Api.Middleware.Exceptions;
using Lueben.Microservice.Api.Middleware.Middleware;

namespace Lueben.Microservice.Api.Middleware.Tests
{
    public class EntityExceptionMiddlewareTests
    {
        [Fact]
        public void GivenGetErrorResult_WhenEntityNotFoundException_ThenExpectedResultIsReturned()
        {
            var middleware = new EntityExceptionMiddleware();

            var exception = new EntityNotFoundException();

            var errorResult = middleware.GetErrorResult(exception);

            Assert.Equal(HttpStatusCode.NotFound, errorResult.StatusCode);
            Assert.Equal(ErrorNames.EntityNotFound, errorResult.Name);
        }

        [Fact]
        public void GivenGetErrorResult_WhenNotEntityNotFoundException_ThenNullIsReturned()
        {
            var middleware = new EntityExceptionMiddleware();

            var exception = new Exception();

            var errorResult = middleware.GetErrorResult(exception);

            Assert.Null(errorResult);
        }
    }
}
