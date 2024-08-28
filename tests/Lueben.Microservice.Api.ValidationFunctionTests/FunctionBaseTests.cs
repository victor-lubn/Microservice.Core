using FluentValidation;
using Lueben.Microservice.Api.ValidationFunction.Exceptions;
using Lueben.Microservice.Api.ValidationFunctionTests.Models;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Moq;
using Newtonsoft.Json;
using System.Text;

namespace Lueben.Microservice.Api.ValidationFunction.Tests
{
    public class FunctionBaseTests
    {
        private readonly TestClassValidator _validator;
        private readonly FunctionBase<TestClass> _function;

        public FunctionBaseTests()
        {
            _validator = new TestClassValidator();
            _function = new FunctionBase<TestClass>(_validator);
        }

        [Fact]
        public async Task GivenGetValidatedRequest_WhenCalledAndRequestIsWithoutBody_ThenJsonBodyNotValidExceptionIsThrown()
        {
            var request = CreateHttpRequest("test object");

            await Assert.ThrowsAsync<JsonBodyNotValidException>(async () => await _function.GetValidatedRequest(request, true));
        }

        [Fact]
        public async Task GivenGetValidatedRequest_WhenCalledAndRequestIsValid_ThenFormValueIsReturned()
        {
            var body = new TestClass
            {
                TestProperty = 20
            };
            var request = CreateHttpRequest(body);

            var result = await _function.GetValidatedRequest(request, true);

            Assert.NotNull(result);
            Assert.Equal(body.Id, result.Id);
        }

        [Fact]
        public async Task GivenGetValidatedRequest_WhenCalledAndRequestIsNotValid_ThenModelNotValidExceptionIsThrown()
        {
            var body = new TestClass();
            var request = CreateHttpRequest(body);

            await Assert.ThrowsAsync<ModelNotValidException>(async () => await _function.GetValidatedRequest(request, true));
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

    public class TestClassValidator : AbstractValidator<TestClass>
    {
        public TestClassValidator()
        {
            RuleFor(x => x.TestProperty)
                .GreaterThan(0)
                .NotNull();
        }
    }
}
