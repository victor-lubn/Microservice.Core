using FluentValidation;
using FluentValidation.Results;
using Lueben.Microservice.Api.ValidationFunction.Exceptions;
using Lueben.Microservice.Api.ValidationFunction.Extensions;
using Lueben.Microservice.Api.ValidationFunctionTests.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using Newtonsoft.Json;
using System.Text;

namespace Lueben.Microservice.Api.ValidationFunction.Tests
{
    public class HttpRequestExtensionsTests
    {
        [Fact]
        public async Task GivenGetRequestValidatedResult_WhenIsCalledAndNoRequestBody_ThenJsonBodyNotValidExceptionIsThrown()
        {
            var request = CreateHttpRequest("test");
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<TestEmptyClass>>();
            validatorMock.Setup(x => x.ValidateAsync(It.IsAny<TestEmptyClass>(), default)).ReturnsAsync(validationResult);

            await Assert.ThrowsAsync<JsonBodyNotValidException>(async () => await request.GetRequestValidatedResult(validatorMock.Object));
        }

        [Fact]
        public async Task GivenGetRequestValidatedResult_WhenIsCalledAndBodyObjectIsEmpty_ThenModelDoesNotHaveAnyPropertiesExceptionIsThrown()
        {
            var body = new TestClass();
            var request = CreateHttpRequest(body);
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<TestEmptyClass>>();
            validatorMock.Setup(x => x.ValidateAsync(It.IsAny<TestEmptyClass>(), default)).ReturnsAsync(validationResult);

            await Assert.ThrowsAsync<ModelDoesNotHaveAnyPropertiesException>(async () => await request.GetRequestValidatedResult(validatorMock.Object, false));
        }

        [Fact]
        public async Task GivenGetRequestValidatedResult_WhenIsCalledAndEmptyBodyIsAllowed_ThenValidationResultIsReturned()
        {
            var body = new TestEmptyClass();
            var request = CreateHttpRequest(body);
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<TestEmptyClass>>();
            validatorMock.Setup(x => x.ValidateAsync(It.IsAny<TestEmptyClass>(), default)).ReturnsAsync(validationResult);

            var result = await request.GetRequestValidatedResult(validatorMock.Object, true);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GivenGetRequestValidatedResult_WhenIsCalledAndRequestBodyCorrect_ThenValidationResultIsReturned()
        {
            var body = new TestClass();
            var request = CreateHttpRequest(body);
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<TestClass>>();
            validatorMock.Setup(x => x.ValidateAsync(It.IsAny<TestClass>(), default)).ReturnsAsync(validationResult);

            var result = await request.GetRequestValidatedResult(validatorMock.Object, true);

            Assert.NotNull(result);
        }

        private HttpRequestData CreateHttpRequest(object? body)
        {
            var context = new Mock<FunctionContext>();

            var request = new Mock<HttpRequestData>(context.Object);

            if (body == null)
            {
                return request.Object;
            }

            var json = JsonConvert.SerializeObject(body);
            var bodyDataStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            request.Setup(r => r.Body).Returns(bodyDataStream);

            return request.Object;
        }
    }
}
