using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Lueben.Microservice.Api.Idempotency.Exceptions;
using Lueben.Microservice.Api.Idempotency.IdempotencyDataProviders;
using Lueben.Microservice.Api.Idempotency.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AutoFixture;
using AutoFixture.AutoMoq;
using System.Collections.Generic;

namespace Lueben.Microservice.Api.Idempotency.Tests
{
    public class AzureTableIdempotencyDataProviderTests
    {
        private const string IdempotencyKey = "Test idemportency key";
        private const string Hash = "Test hash";
        private const string FunctionName = "Test function name";

        private readonly Mock<TableClient> _tableClientMock;
        private readonly AzureTableIdempotencyDataProvider _provider;
        private readonly IFixture _fixture;

        public AzureTableIdempotencyDataProviderTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _tableClientMock = new Mock<TableClient>();
            var loggerMock = new Mock<ILogger<AzureTableIdempotencyDataProvider>>();

            _provider = new AzureTableIdempotencyDataProvider(loggerMock.Object, _tableClientMock.Object);
        }

        [Fact]
        public async Task GivenAdd_WhenCalled_ThenEntityIsAdded()
        {
            await _provider.Add(IdempotencyKey, Hash, FunctionName);

            _tableClientMock.Verify(x =>
                x.AddEntityAsync(
                    It.Is<IdempotencyEntity>(e =>
                        e.PayloadHash == Hash && e.PartitionKey == IdempotencyKey && e.RowKey == IdempotencyKey &&
                        e.FunctionName == FunctionName), default), Times.Once());
        }

        [Fact]
        public async Task GivenAdd_WhenCalledAndEntityAlreadyExists_ThenIdempotencyConflictExceptionIsThrown()
        {
            _tableClientMock.Setup(x =>
                x.AddEntityAsync(
                    It.Is<IdempotencyEntity>(e =>
                        e.PayloadHash == Hash && e.PartitionKey == IdempotencyKey && e.RowKey == IdempotencyKey &&
                        e.FunctionName == FunctionName), default)).ThrowsAsync(new RequestFailedException(409, "test"));

            var exception = await Assert.ThrowsAsync<IdempotencyConflictException>(async () => await _provider.Add(IdempotencyKey, Hash, FunctionName));

            Assert.NotNull(exception);
        }

        [Fact]
        public async Task GivenDelete_WhenCalled_ThenEntityIsDeleted()
        {
            var idemportencyEntity = new IdempotencyEntity
            {
                FunctionName = FunctionName,
                RowKey = IdempotencyKey,
                PartitionKey = IdempotencyKey
            };

            await _provider.Delete(idemportencyEntity);

            _tableClientMock.Verify(x =>
                x.DeleteEntityAsync(IdempotencyKey, IdempotencyKey, idemportencyEntity.ETag, default), Times.Once());
        }

        [Fact]
        public async Task GivenUpdate_WhenCalled_ThenEntityIsUpdated()
        {
            var idemportencyEntity = new IdempotencyEntity
            {
                FunctionName = FunctionName,
                RowKey = IdempotencyKey,
                PartitionKey = IdempotencyKey
            };

            await _provider.Update(idemportencyEntity);

            _tableClientMock.Verify(x =>
                x.UpdateEntityAsync(idemportencyEntity, idemportencyEntity.ETag, default, default), Times.Once());
        }

        [Fact]
        public async Task GivenGet_WhenCalled_ThenEntityIsReturned()
        {
            var idemportencyEntity = new IdempotencyEntity
            {
                FunctionName = FunctionName,
                RowKey = IdempotencyKey,
                PartitionKey = IdempotencyKey
            };
            var responseMock = new Mock<Response>();
            _tableClientMock.Setup(x =>
                    x.GetEntityAsync<IdempotencyEntity>(IdempotencyKey, IdempotencyKey, default, default))
                .Returns(Task.FromResult(Response.FromValue(idemportencyEntity, responseMock.Object)));

            var actualEntity = await _provider.Get(IdempotencyKey);

            _tableClientMock.Verify(x =>
                x.GetEntityAsync<IdempotencyEntity>(IdempotencyKey, IdempotencyKey, default, default), Times.Once());

            Assert.Equal(idemportencyEntity, actualEntity);
        }

        [Fact]
        public async Task GivenGet_WhenCalledAndNoEntityFound_ThenNullIsReturned()
        {
            _tableClientMock.Setup(x =>
                    x.GetEntityAsync<IdempotencyEntity>(IdempotencyKey, IdempotencyKey, default, default))
                .ThrowsAsync(new RequestFailedException(404, "test"));

            var actualEntity = await _provider.Get(IdempotencyKey);

            _tableClientMock.Verify(x =>
                x.GetEntityAsync<IdempotencyEntity>(IdempotencyKey, IdempotencyKey, default, default), Times.Once());

            Assert.Null(actualEntity);
        }

        [Fact]
        public async Task GivenCleanUp_WhenCalled_ThenExistingRecordsAreRequestedAndDeleted()
        {
            var idemportencyEntity = new IdempotencyEntity
            {
                FunctionName = FunctionName,
                RowKey = IdempotencyKey,
                PartitionKey = IdempotencyKey
            };
            var entities = new[] { idemportencyEntity};
            var page = Page<IdempotencyEntity>.FromValues(entities, continuationToken: null, Mock.Of<Response>());
            var pageable = Pageable<IdempotencyEntity>.FromPages(new[] { page });
            _tableClientMock
                .Setup(x => x.Query<IdempotencyEntity>(It.IsAny<string>(), 1000, It.IsAny<IEnumerable<string>>(), default))
            .Returns(pageable);

            await _provider.CleanUp();

            _tableClientMock.Verify(x =>
                x.DeleteEntityAsync(IdempotencyKey, IdempotencyKey, idemportencyEntity.ETag, default), Times.Once());
        }
    }
}
