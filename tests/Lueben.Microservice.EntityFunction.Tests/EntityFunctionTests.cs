using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Lueben.Microservice.Api.ValidationFunction.Exceptions;
using Lueben.Microservice.EntityFunction.Models;
using Lueben.Microservice.Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Xunit;
using Microsoft.Azure.Functions.Worker;
using System.IO;
using System.Text;

namespace Lueben.Microservice.EntityFunction.Tests
{
    public class EntityFunctionTests
    {
        private readonly IServiceCollection _serviceCollection;

        public EntityFunctionTests()
        {
            _serviceCollection = SetupServiceProvider();
        }

        [Fact]
        public async Task GivenCreateMethod_WhenCommandIsExecuted_ThenCreatedStatusCodeShouldBeReturnedWithGeneratedId()
        {
            const long mockId = 1;

            var mockMediator = new Mock<IMediator>();
            mockMediator
                .Setup(m => m.Send<CreateTestCommand, long>(It.IsAny<CreateTestCommand>()))
                .Callback<IRequest<long>>(req =>
                {
                    var command = req as CreateTestCommand;

                    Assert.NotNull(command);
                })
                .Returns(() => Task.FromResult(mockId));

            var request = CreateHttpRequest(true);

            await using var provider = _serviceCollection.BuildServiceProvider();
            var testFunction = CreateTestFunction(mockMediator, provider);
            var response = (CreatedObjectResult<long>)await testFunction.Create(request);

            Assert.NotNull(response);
            Assert.Equal(mockId, response.Data);
            Assert.Equal((int)HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task GivenCreateWithoutResponseMethod_WhenCommandIsExecuted_ThenCreatedStatusCodeShouldBeReturnedWithGeneratedId()
        {
            var mockMediator = new Mock<IMediator>();
            mockMediator
                .Setup(m => m.Send<TestCommand, Unit>(It.IsAny<TestCommand>()))
                .Callback<IRequest<Unit>>(req =>
                {
                    var command = req as TestCommand;

                    Assert.NotNull(command);
                })
                .Returns(() => Task.FromResult(Unit.Value));

            var request = CreateHttpRequest(true);

            await using var provider = _serviceCollection.BuildServiceProvider();
            var testFunction = CreateTestFunction(mockMediator, provider);
            var response = (CreatedEmptyResult)await testFunction.CreateWithoutResponse(request);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task GivenGetAllMethod_WhenCommandIsExecuted_ThenTheCollectionOfElementsShouldBeReturned()
        {
            var mockData = new List<TestEntity>
            {
                new(),
                new()
            };

            var mockMediator = new Mock<IMediator>();
            mockMediator
                .Setup(m => m.Send<GetAllTestQuery, IQueryable<TestEntity>>(It.IsAny<GetAllTestQuery>()))
                .Returns(() => Task.FromResult(mockData.AsQueryable()));

            var request = CreateHttpRequest(false);

            await using var provider = _serviceCollection.BuildServiceProvider();
            var testFunction = CreateTestFunction(mockMediator, provider);
            var response = (GetJsonResult<IList<TestModel>>)await testFunction.GetAll();

            var collection = (IList<TestModel>)response.Value;

            Assert.NotNull(response);
            Assert.NotEmpty(collection);
            Assert.Equal(2, collection.Count);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GivenGetMethod_WhenCommandIsExecuted_ThenTheOnlyOneElementShouldBeReturned()
        {
            var mockEntity = new TestEntity();

            var mockMediator = new Mock<IMediator>();
            mockMediator
                .Setup(m => m.Send<GetTestQuery, TestEntity>(It.IsAny<GetTestQuery>()))
                .Returns(() => Task.FromResult(mockEntity));

            var request = CreateHttpRequest(false);

            await using var provider = _serviceCollection.BuildServiceProvider();
            var testFunction = CreateTestFunction(mockMediator, provider);
            var response = (GetJsonResult<TestModel>)await testFunction.Get(1);

            var model = (TestModel)response.Value;

            Assert.NotNull(response);
            Assert.NotNull(model);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GivenGetPaginatedEntitiesMethod_WhenCommandIsExecuted_ThenPageOfEntitiesShouldBeReturned()
        {
            var mockData = new List<TestEntity>
            {
                new(),
                new()
            };

            var mockMediator = new Mock<IMediator>();
            mockMediator
                .Setup(m => m.Send<GetAllPaginatedTestQuery, GetAllEntitiesResult<TestEntity>>(It.IsAny<GetAllPaginatedTestQuery>()))
                .Returns(() => Task.FromResult(new GetAllEntitiesResult<TestEntity>(mockData.AsQueryable(), 2, 1)));

            var request = CreateHttpRequest(false);

            await using var provider = _serviceCollection.BuildServiceProvider();
            var testFunction = CreateTestFunction(mockMediator, provider);
            var response = (GetListJsonResult<TestModel>)await testFunction.GetAllPaginatedEntities(request);

            var collection = response.Items;

            Assert.NotNull(response);
            Assert.NotEmpty(collection);
            Assert.Equal(2, response.TotalItems);
            Assert.Equal(2, response.TotalPages);
            Assert.Equal(2, collection.Count);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GivenPatchMethod_WhenCommandIsExecuted_ThenNoContentStatusCodeShouldBeReturned()
        {
            var mockMediator = new Mock<IMediator>();
            mockMediator
                .Setup(m => m.Send<TestCommand, Unit>(It.IsAny<TestCommand>()))
                .Callback<IRequest<Unit>>(req =>
                {
                    var command = req as TestCommand;

                    Assert.NotNull(command);
                })
                .Returns(() => Task.FromResult(Unit.Value));

            var request = CreateHttpRequest(true);

            await using var provider = _serviceCollection.BuildServiceProvider();
            var testFunction = CreateTestFunction(mockMediator, provider);
            var response = (NoContentResult)await testFunction.Patch(request, 1);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GivenPutMethod_WhenCommandIsExecuted_ThenNoContentStatusCodeShouldBeReturned()
        {
            var mockMediator = new Mock<IMediator>();
            mockMediator
                .Setup(m => m.Send<TestCommand, Unit>(It.IsAny<TestCommand>()))
                .Callback<IRequest<Unit>>(req =>
                {
                    var command = req as TestCommand;

                    Assert.NotNull(command);
                })
                .Returns(() => Task.FromResult(Unit.Value));

            var request = CreateHttpRequest(true);

            await using var provider = _serviceCollection.BuildServiceProvider();
            var testFunction = CreateTestFunction(mockMediator, provider);
            var response = (NoContentResult)await testFunction.Put(request, 1);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GivenPutMethodWithValidatorParameter_WhenCommandIsExecuted_ThenNoContentStatusCodeShouldBeReturned()
        {
            var mockMediator = new Mock<IMediator>();
            mockMediator
                .Setup(m => m.Send<TestCommand, Unit>(It.IsAny<TestCommand>()))
                .Callback<IRequest<Unit>>(req =>
                {
                    var command = req as TestCommand;

                    Assert.NotNull(command);
                })
                .Returns(() => Task.FromResult(Unit.Value));

            var validatorMock = new Mock<AbstractValidator<TestModel>>();
            validatorMock.Setup(m => m.ValidateAsync(It.IsAny<ValidationContext<TestModel>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var request = CreateHttpRequest(true);

            await using var provider = _serviceCollection.BuildServiceProvider();
            var testFunction = CreateTestFunction(mockMediator, provider);
            var response = (NoContentResult)await testFunction.Put(request, 1, validatorMock.Object);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GivenPutMethodWithValidatorParameter_WhenCommandIsExecuted_ThenOkStatusCodeShouldBeReturned()
        {
            var mockMediator = new Mock<IMediator>();
            mockMediator
                .Setup(m => m.Send<TestCommand, Unit>(It.IsAny<TestCommand>()))
                .Callback<IRequest<Unit>>(req =>
                {
                    var command = req as TestCommand;

                    Assert.NotNull(command);
                })
                .Returns(() => Task.FromResult(Unit.Value));

            var validatorMock = new Mock<AbstractValidator<TestModel>>();
            validatorMock.Setup(m => m.ValidateAsync(It.IsAny<ValidationContext<TestModel>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var request = CreateHttpRequest(true);

            await using var provider = _serviceCollection.BuildServiceProvider();
            var testFunction = CreateTestFunction(mockMediator, provider);
            var response = (ObjectResult<Unit>)await testFunction.PutWithUnitResult(request, 1, validatorMock.Object);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GivenDeleteMethod_WhenCommandIsExecuted_ThenNoContentStatusCodeShouldBeReturned()
        {
            var mockMediator = new Mock<IMediator>();
            mockMediator
                .Setup(m => m.Send<TestCommand, Unit>(It.IsAny<TestCommand>()))
                .Callback<IRequest<Unit>>(req =>
                {
                    var command = req as TestCommand;

                    Assert.NotNull(command);
                })
                .Returns(() => Task.FromResult(Unit.Value));

            var request = CreateHttpRequest(false);

            await using var provider = _serviceCollection.BuildServiceProvider();
            var testFunction = CreateTestFunction(mockMediator, provider);
            var response = (NoContentResult)await testFunction.Delete(1);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GivenDeleteGenericMethod_WhenCommandIsExecuted_ThenOkStatusCodeAndObjectResultShouldBeReturned()
        {
            var mockMediator = new Mock<IMediator>();
            mockMediator
                .Setup(m => m.Send<TestCommand, Unit>(It.IsAny<TestCommand>()))
                .Callback<IRequest<Unit>>(req =>
                {
                    var command = req as TestCommand;

                    Assert.NotNull(command);
                })
                .Returns(() => Task.FromResult(Unit.Value));

            var request = CreateHttpRequest(false);

            await using var provider = _serviceCollection.BuildServiceProvider();
            var testFunction = CreateTestFunction(mockMediator, provider);
            var response = (ObjectResult<Unit>)await testFunction.DeleteGeneric(1);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GivenGetValidatedRequestMethod_WhenModelIsValid_ThenShouldReturnModel()
        {
            var mockMediator = new Mock<IMediator>();

            var request = CreateHttpRequest(true);

            await using var provider = _serviceCollection.BuildServiceProvider();
            var testFunction = CreateTestFunction(mockMediator, provider);

            var validatorMock = new Mock<AbstractValidator<TestModel>>();
            validatorMock.Setup(m => m.ValidateAsync(It.IsAny<ValidationContext<TestModel>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            var model = await testFunction.GetValidatedRequest<TestModel>(request, validatorMock.Object);

            Assert.NotNull(model);
        }

        [Fact]
        public async Task GivenGetValidatedRequestMethod_WhenModelIsNotValid_ThenShouldThrowException()
        {
            var mockMediator = new Mock<IMediator>();
            var request = CreateHttpRequest(true);

            await using var provider = _serviceCollection.BuildServiceProvider();
            var testFunction = CreateTestFunction(mockMediator, provider);

            var validationError = new ValidationFailure("Test", "Should not be null")
            {
                FormattedMessagePlaceholderValues = new Dictionary<string, object> { ["PropertyName"] = "Test" }
            };
            var validatorMock = new Mock<AbstractValidator<TestModel>>();
            validatorMock.Setup(m => m.ValidateAsync(It.IsAny<ValidationContext<TestModel>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { validationError }));

            await Assert.ThrowsAsync<ModelNotValidException>(async () => await testFunction.GetValidatedRequest<TestModel>(request, validatorMock.Object));
        }

        private static IServiceCollection SetupServiceProvider()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient<IHandlerProvider, HandlerProvider>(provider => new HandlerProvider(provider));
            serviceCollection.AddAutoMapper(typeof(TestMappingProfile).Assembly);

            return serviceCollection;
        }

        private TestFunction CreateTestFunction(Mock<IMediator> mockMediator, ServiceProvider provider)
        {
            var testFunction = new TestFunction(mockMediator.Object, provider.GetService<IMapper>());

            return testFunction;
        }

        private HttpRequestData CreateHttpRequest(bool hasBody)
        {
            var context = new Mock<FunctionContext>();

            var request = new Mock<HttpRequestData>(context.Object);

            if (hasBody)
            {
                var json = JsonConvert.SerializeObject(new TestModel
                {
                    Test = "test"
                });
                var bodyDataStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                request.Setup(r => r.Body).Returns(bodyDataStream);
            }

            return request.Object;
        }
    }
}
