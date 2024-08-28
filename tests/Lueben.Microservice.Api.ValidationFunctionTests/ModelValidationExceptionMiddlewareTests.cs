using Lueben.Microservice.Api.ValidationFunction.Exceptions;
using Lueben.Microservice.Api.ValidationFunction.Middleware;
using System.Net;
using Lueben.Microservice.Api.Middleware.Constants;

namespace Lueben.Microservice.Api.ValidationFunction.Tests
{
    public class ModelValidationExceptionMiddlewareTests
    {
        [Fact]
        public void GivenGetErrorResult_WhenIsCalled_ThenExpectedResultIsReturned()
        {
            var middleware = new ModelValidationExceptionMiddleware();

            var exception = new ModelNotValidException("test field", "test validation error");

            var errorResult = middleware.GetErrorResult(exception);

            Assert.Equal(HttpStatusCode.BadRequest, errorResult.StatusCode);
            Assert.Equal(ErrorNames.ModelNotValid, errorResult.Name);
        }
    }
}
