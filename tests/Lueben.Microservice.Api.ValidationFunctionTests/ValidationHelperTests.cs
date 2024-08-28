using FluentValidation;
using FluentValidation.Results;
using Lueben.Microservice.Api.ValidationFunction.Exceptions;
using Lueben.Microservice.Api.ValidationFunctionTests.Models;
using Moq;
using Newtonsoft.Json;

namespace Lueben.Microservice.Api.ValidationFunction.Tests
{
    public class ValidationHelperTests
    {
        [Fact]
        public async Task GivenGetValidatedResult_WhenCalledAndResultIsValid_ThenNoErrorsAreReturned()
        {
            var objectToValidate = new TestClass();
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<TestClass>>();
            validatorMock.Setup(x => x.ValidateAsync(objectToValidate, default)).ReturnsAsync(validationResult);

            var result = await ValidationHelper.GetValidatedResult(validatorMock.Object, objectToValidate);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task GivenGetValidatedResult_WhenCalledAndResultIsNotValid_ThenErrorsAreReturned()
        {
            var expectedError = new ValidationFailure
            {
                ErrorMessage = "test error message",
                FormattedMessagePlaceholderValues = new Dictionary<string, object>
                {
                    {"PropertyName", "Test property"}
                }
            };
            var objectToValidate = new TestClass();
            var validationResult = new ValidationResult
            {
                Errors = new List<ValidationFailure> { expectedError }
            };
            var validatorMock = new Mock<IValidator<TestClass>>();
            validatorMock.Setup(x => x.ValidateAsync(objectToValidate, default)).ReturnsAsync(validationResult);

            var result = await ValidationHelper.GetValidatedResult(validatorMock.Object, objectToValidate);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
        }

        [Fact]
        public void GivenDeserializeObject_WhenCalledAndNoErrors_ThenExpectedObjectIsReturned()
        {
            var objectToDeserialize = new TestClass();
            var objectString = JsonConvert.SerializeObject(objectToDeserialize);

            var result = ValidationHelper.DeserializeObject<TestClass>(objectString);

            Assert.Equal(objectToDeserialize.Id, result.Id);
        }

        [Fact]
        public void GivenDeserializeObject_WhenCalledAndDeserializationErrors_ThenJsonBodyNotValidExceptionIsThrown()
        {
            var objectString = JsonConvert.SerializeObject("test");

            Assert.Throws<JsonBodyNotValidException>(() => ValidationHelper.DeserializeObject<TestClass>(objectString));
        }
    }
}