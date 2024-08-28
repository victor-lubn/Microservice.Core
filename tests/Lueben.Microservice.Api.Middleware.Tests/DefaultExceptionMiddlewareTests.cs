using System.Net;
using Lueben.Microservice.Api.Middleware.Constants;
using Lueben.Microservice.Api.Middleware.Middleware;

namespace Lueben.Microservice.Api.Middleware.Tests
{
    public class DefaultExceptionMiddlewareTests
    {
        [Fact]
        public void GivenGetErrorResult_WhenIsCalled_ThenExpectedResultIsReturned()
        {
            var middleware = new DefaultExceptionMiddleware();

            var exception = new Exception();

            var errorResult = middleware.GetErrorResult(exception);

            Assert.Equal(HttpStatusCode.InternalServerError, errorResult.StatusCode);
            Assert.Equal(ErrorNames.InternalServerError, errorResult.Name);
        }
    }
}
