using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Lueben.Microservice.Api.Idempotency.Constants;
using Lueben.Microservice.Api.Idempotency.Exceptions;
using Lueben.Microservice.Api.Idempotency.Extensions;
using Lueben.Microservice.Api.Idempotency.FunctionWrappers;
using Lueben.Microservice.Api.Idempotency.IdempotencyDataProviders;
using Lueben.Microservice.Api.Idempotency.Models;
using Lueben.Microservice.Api.Idempotency.Tests.Extensions;
using Lueben.Microservice.Api.ValidationFunction.Exceptions;
using Lueben.Microservice.EntityFunction.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Lueben.Microservice.Api.Idempotency.Tests.FunctionWrappers
{
    public class IdempotencyFunctionWrapperTests
    {
        private readonly ILogger<IdempotencyFunctionWrapper> _logger;

        public IdempotencyFunctionWrapperTests()
        {
            _logger = Mock.Of<ILogger<IdempotencyFunctionWrapper>>();
        }

        [Fact]
        public async Task
            GivenExecuteMethod_WhenResponseValueIsNullAndIdempotencyEntityAlreadyExists_ThenReturnCreatedResult()
        {
            var idempotencyKey = Guid.NewGuid().ToString();

            var context = CreateHttpContext(true);
            var request = context.Request;
            request.Headers.Add(Headers.Idempotency, idempotencyKey);

            var mockIdempotencyDataProvider = new Mock<IIdempotencyDataProvider<IdempotencyEntity>>();
            mockIdempotencyDataProvider.Setup(x => x.Get(idempotencyKey))
                .ReturnsAsync(CreateIdempotencyEntity(request, typeof(CreatedEmptyResult).ToString()));

            var functionWrapper = CreateIdempotencyFunctionWrapper(context, mockIdempotencyDataProvider);

            var response = await functionWrapper.Execute(() =>
                Task.FromResult((IActionResult) new CreatedEmptyResult()), "testFunction");

            var result = response as ObjectResult;

            Assert.NotNull(result);
            Assert.Null(result.Value);
            Assert.Equal((int) HttpStatusCode.Created, result.StatusCode);

            mockIdempotencyDataProvider.Verify(x => x.Get(idempotencyKey), Times.Once);
        }

        [Fact]
        public async Task GivenExecuteMethod_WhenResponseValueIsSomeObjectAndIdempotencyEntityAlreadyExists_ThenReturnCreatedResult()
        {
            var idempotencyKey = Guid.NewGuid().ToString();

            var context = CreateHttpContext(true);
            var request = context.Request;
            request.Headers.Add(Headers.Idempotency, idempotencyKey);

            var mockResponse = new MockResponse
            {
                Name = "test"
            };

            var mockIdempotencyDataProvider = new Mock<IIdempotencyDataProvider<IdempotencyEntity>>();
            mockIdempotencyDataProvider.Setup(x => x.Get(idempotencyKey))
                .ReturnsAsync(CreateIdempotencyEntity(request, typeof(MockResponse).ToString(), mockResponse));

            var functionWrapper = CreateIdempotencyFunctionWrapper(context, mockIdempotencyDataProvider);

            var response = await functionWrapper.Execute(() =>
                Task.FromResult((IActionResult)new ObjectResult(mockResponse)), "testFunction");

            var result = response as ObjectResult;
            var value = result?.Value as MockResponse;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
            Assert.IsType<MockResponse>(result.Value);
            Assert.Equal(mockResponse.Name, value?.Name);

            mockIdempotencyDataProvider.Verify(x => x.Get(idempotencyKey), Times.Once);
        }

        [Fact]
        public async Task GivenExecuteMethod_WhenResponseValueIsNullAndIdempotencyEntityIsNew_ThenReturnOkResult()
        {
            var idempotencyKey = Guid.NewGuid().ToString();

            var context = CreateHttpContext(true);
            var request = context.Request;
            request.Headers.Add(Headers.Idempotency, idempotencyKey);

            var mockIdempotencyDataProvider = new Mock<IIdempotencyDataProvider<IdempotencyEntity>>();
            mockIdempotencyDataProvider.SetupSequence(x => x.Get(idempotencyKey))
                .ReturnsAsync((IdempotencyEntity)null)
                .ReturnsAsync(CreateIdempotencyEntity(request, typeof(MockResponse).ToString()));

            var functionWrapper = CreateIdempotencyFunctionWrapper(context, mockIdempotencyDataProvider);

            var response = await functionWrapper.Execute(() =>
                Task.FromResult((IActionResult)new CreatedEmptyResult()), "testFunction");

            var result = response as ObjectResult;

            Assert.NotNull(result);
            Assert.Null(result.Value);
            Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);

            mockIdempotencyDataProvider.Verify(x => x.Get(idempotencyKey), Times.Exactly(2));
        }

        [Fact]
        public async Task GivenExecuteMethod_WhenResponseValueIsSomeObjectAndIdempotencyEntityIsNew_ThenReturnOkResult()
        {
            var idempotencyKey = Guid.NewGuid().ToString();

            var context = CreateHttpContext(true);
            var request = context.Request;
            request.Headers.Add(Headers.Idempotency, idempotencyKey);

            var mockResponse = new MockResponse
            {
                Name = "test"
            };

            var mockIdempotencyDataProvider = new Mock<IIdempotencyDataProvider<IdempotencyEntity>>();
            mockIdempotencyDataProvider.SetupSequence(x => x.Get(idempotencyKey))
                .ReturnsAsync((IdempotencyEntity)null)
                .ReturnsAsync(CreateIdempotencyEntity(request, typeof(MockResponse).ToString(), mockResponse));

            var functionWrapper = CreateIdempotencyFunctionWrapper(context, mockIdempotencyDataProvider);

            var response = await functionWrapper.Execute(() =>
                Task.FromResult((IActionResult)new ObjectResult(mockResponse)), "testFunction");

            var result = response as ObjectResult;
            var value = result?.Value as MockResponse;

            var body = await request.Body.ReadFullyAsync();

            Assert.NotEmpty(body);
            Assert.NotNull(result);
            Assert.IsType<MockResponse>(result.Value);
            Assert.Equal(mockResponse.Name, value?.Name);

            mockIdempotencyDataProvider.Verify(x => x.Get(idempotencyKey), Times.Exactly(2));
        }

        [Fact]
        public async Task GivenExecuteMethod_WhenIdempotencyHeaderDoesNotExists_ThenIdempotencyKeyNullOrEmptyExceptionIsThrown()
        {
            var context = CreateHttpContext(true);

            var mockIdempotencyDataProvider = new Mock<IIdempotencyDataProvider<IdempotencyEntity>>();

            var functionWrapper = CreateIdempotencyFunctionWrapper(context, mockIdempotencyDataProvider);

            var exception = await Assert.ThrowsAsync<IdempotencyKeyNullOrEmptyException>(() =>
                functionWrapper.Execute(() =>
                    Task.FromResult((IActionResult)new CreatedEmptyResult()), "testFunction"));

            Assert.Equal(ErrorMessages.IdempotencyKeyNullOrEmptyError, exception.Message);
        }

        [Fact]
        public async Task GivenExecuteMethod_WhenIdempotencyHeaderDoesNotExistsAndIdempotencyIsOptional_ThenReturnOkResult()
        {
            var context = CreateHttpContext(true);

            var mockIdempotencyDataProvider = new Mock<IIdempotencyDataProvider<IdempotencyEntity>>();

            var mockResponse = new MockResponse
            {
                Name = "test"
            };

            var functionWrapper = CreateIdempotencyFunctionWrapper(context, mockIdempotencyDataProvider);

            var response = await functionWrapper.Execute(() =>
                    Task.FromResult((IActionResult)new ObjectResult(mockResponse) { StatusCode = (int)HttpStatusCode.OK }), "testFunction", isIdempotencyOptional:true);

            var result = response as ObjectResult;
            var value = result?.Value as MockResponse;
            var body = await context.Request.Body.ReadFullyAsync();

            Assert.NotEmpty(body);
            Assert.NotNull(result);
            Assert.IsType<MockResponse>(result.Value);
            Assert.Equal(mockResponse.Name, value?.Name);

            mockIdempotencyDataProvider.Verify(x => x.Get(It.IsAny<string>()), Times.Never);

        }

        [Fact]
        public async Task GivenExecuteMethod_WhenIdempotencyHeaderHasInvalidFormat_ThenModelNotValidExceptionIsThrown()
        {
            var context = CreateHttpContext(true);
            var request = context.Request;
            request.Headers.Add(Headers.Idempotency, "test");

            var mockIdempotencyDataProvider = new Mock<IIdempotencyDataProvider<IdempotencyEntity>>();

            var functionWrapper = CreateIdempotencyFunctionWrapper(context, mockIdempotencyDataProvider);

            var exception = await Assert.ThrowsAsync<ModelNotValidException>(() =>
                functionWrapper.Execute(() =>
                    Task.FromResult((IActionResult)new CreatedEmptyResult()), "testFunction"));

            var validationError = exception.ValidationErrors.First();

            Assert.Equal("Model is not valid.", exception.Message);
            Assert.Equal("header", validationError.Location);
            Assert.Equal(Headers.Idempotency, validationError.Field);
            Assert.Equal(ErrorMessages.IdempotencyKeyNotValidError, validationError.Issue);
        }

        [Fact]
        public async Task GivenExecuteMethod_WhenBodyIsEmpty_ThenJsonBodyNotValidExceptionIsThrown()
        {
            var context = CreateHttpContext(false);
            var request = context.Request;
            request.Headers.Add(Headers.Idempotency, Guid.NewGuid().ToString());

            var mockIdempotencyDataProvider = new Mock<IIdempotencyDataProvider<IdempotencyEntity>>();

            var functionWrapper = CreateIdempotencyFunctionWrapper(context, mockIdempotencyDataProvider);

            var exception = await Assert.ThrowsAsync<JsonBodyNotValidException>(() =>
                functionWrapper.Execute(() =>
                    Task.FromResult((IActionResult)new CreatedEmptyResult()), "testFunction"));

            var validationError = exception.ValidationErrors.First();

            Assert.Equal("Model is not valid.", exception.Message);
            Assert.Equal("body", validationError.Location);
            Assert.Equal("body", validationError.Field);
            Assert.Equal(ErrorMessages.EmptyBodyError, validationError.Issue);
        }

        private IFunctionWrapper CreateIdempotencyFunctionWrapper(DefaultHttpContext context, Mock<IIdempotencyDataProvider<IdempotencyEntity>> mockIdempotencyDataProvider)
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);

            var idempotencyFunctionWrapper = new IdempotencyFunctionWrapper(mockHttpContextAccessor.Object, mockIdempotencyDataProvider.Object, _logger);

            return idempotencyFunctionWrapper;
        }

        private IdempotencyEntity CreateIdempotencyEntity(HttpRequest request, string entityType, object response = null)
        {
            var body = request.Body.ReadFully();
            var hash = HashGenerator.CreateShaHash(body);
            var idempotencyKey = request.Headers[Headers.Idempotency];

            var responseString = JsonConvert.SerializeObject(response);

            return new IdempotencyEntity
            {
                PartitionKey = idempotencyKey,
                RowKey = idempotencyKey,
                Timestamp = DateTime.UtcNow,
                EntityType = entityType,
                FunctionName = "testFunction",
                PayloadHash = hash,
                Response = Convert.ToBase64String(Encoding.ASCII.GetBytes(responseString)),
            };
        }

        private class MockResponse
        {
            public string Name { get; set; }
        }

        public DefaultHttpContext CreateHttpContext(bool hasBody)
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            if (hasBody)
            {
                var json = JsonConvert.SerializeObject(new MockResponse
                {
                    Name = "test"
                });
                request.Body = json.GenerateStreamFromString();
            }
            return context;
        }
    }
}