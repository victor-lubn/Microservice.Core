using Lueben.Microservice.Api.Middleware.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Moq;

namespace Lueben.Microservice.Api.Middleware.Tests
{
    public class ExceptionHandlingMiddlewareTests
    {
        private readonly Mock<FunctionContext> _mockContext;
        private readonly Mock<FunctionExecutionDelegate> _mockNext;

        public ExceptionHandlingMiddlewareTests()
        {
            _mockContext = new Mock<FunctionContext>();
            _mockNext = new Mock<FunctionExecutionDelegate>();
        }

        [Fact]
        public async Task GivenExceptionHandlingMiddleware_WhenExecutedAndNoExceptions_ThenShouldCallNextHandler()
        {
            _mockNext.Setup(n => n.Invoke(It.IsAny<FunctionContext>())).Returns(Task.CompletedTask);

            var middleware = new EntityExceptionMiddleware();

            await middleware.Invoke(_mockContext.Object, _mockNext.Object);

            _mockNext.Verify(n => n.Invoke(It.IsAny<FunctionContext>()), Times.Once);
        }

        [Fact]
        public async Task GivenExceptionHandlingMiddleware_WhenExecutedAndUnexpectedExceptionOccurred_ThenShouldThrowTheException()
        {
            _mockNext.Setup(n => n(It.IsAny<FunctionContext>())).ThrowsAsync(new Exception("Test exception"));

            var middleware = new EntityExceptionMiddleware();

            await Assert.ThrowsAsync<Exception>(() => middleware.Invoke(_mockContext.Object, _mockNext.Object));
        }
    }
}