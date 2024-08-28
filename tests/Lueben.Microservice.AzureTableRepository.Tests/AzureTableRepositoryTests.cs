using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Lueben.Microservice.AzureTableRepository.Tests
{
    public class AzureTableRepositoryTests
    {
        private readonly Mock<TableClient> _tableClientMock;

        public AzureTableRepositoryTests()
        {
            _tableClientMock = new Mock<TableClient>(new Uri("https://test.org/test"), new TableClientOptions());
        }

        [Fact]
        public void GivenAzureTableRepository_WhenCreated_ThenProperTableIsCreated()
        {
            const string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
            var tableServiceClientMock = new Mock<TableServiceClient>(connectionString);

            tableServiceClientMock.Setup(x => x.GetTableClient(nameof(FirstTableRecordClass)))
                .Returns(_tableClientMock.Object);

            var tableClientFactoryMock = new Mock<IAzureClientFactory<TableServiceClient>>();
            tableClientFactoryMock.Setup(x => x.CreateClient(Constants.DefaultTableServiceClientName))
                .Returns(tableServiceClientMock.Object);

            var optionsMock = new Mock<IOptionsSnapshot<AzureTableRepositoryOptions>>();
            optionsMock.Setup(x => x.Get(nameof(FirstTableRecordClass))).Returns(new AzureTableRepositoryOptions());

            _ = new AzureTableRepository<FirstTableRecordClass>(tableClientFactoryMock.Object, optionsMock.Object);

            tableClientFactoryMock.Verify(x => x.CreateClient(Constants.DefaultTableServiceClientName), Times.Once);
            _tableClientMock.Verify(x => x.CreateIfNotExists(default), Times.Never);
        }

        [Fact]
        public async void GivenAzureTableRepository_WhenAdd_ThenProperMethodIsCalled()
        {
            var sut = new AzureTableRepository<FirstTableRecordClass>(_tableClientMock.Object);

            await sut.AddAsync(new FirstTableRecordClass());

            _tableClientMock.Verify(x => x.AddEntityAsync(It.IsAny<FirstTableRecordClass>(), default), Times.Once);
            _tableClientMock.Verify(x => x.CreateIfNotExists(default), Times.Once);
        }

        [Fact]
        public async void GivenAzureTableRepository_WhenUpsert_ThenProperMethodIsCalled()
        {
            var sut = new AzureTableRepository<FirstTableRecordClass>(_tableClientMock.Object);

            await sut.UpsertAsync(new FirstTableRecordClass());

            _tableClientMock.Verify(x => x.UpsertEntityAsync(It.IsAny<FirstTableRecordClass>(), TableUpdateMode.Merge, default), Times.Once);
        }

        [Fact]
        public async void GivenAzureTableRepository_WhenGetForExistingRow_ThenRowIsReturned()
        {
            const string partitionKey = "partitionKey";
            const string rowKey = "rowKey";
            var responseMock = new Mock<Response>();
            _tableClientMock.Setup(x => x.GetEntityAsync<FirstTableRecordClass>(It.IsAny<string>(), It.IsAny<string>(), null, default))
                .Returns(Task.FromResult(Response.FromValue(new FirstTableRecordClass(), responseMock.Object)));
            var sut = new AzureTableRepository<FirstTableRecordClass>(_tableClientMock.Object);

            var row = await sut.GetAsync(partitionKey, rowKey);

            Assert.NotNull(row);
            _tableClientMock.Verify(x => x.GetEntityAsync<FirstTableRecordClass>(partitionKey, rowKey, null, default), Times.Once);
        }

        [Fact]
        public async void GivenAzureTableRepository_WhenRemove_ThenProperMethodIsCalled()
        {
            var partitionKey = "testPartitionKey";
            var rowKey = "testRowKey";
            var sut = new AzureTableRepository<FirstTableRecordClass>(_tableClientMock.Object);

            await sut.RemoveAsync(partitionKey, rowKey);

            _tableClientMock.Verify(x => x.DeleteEntityAsync(partitionKey, rowKey, default, default), Times.Once);
        }

        [Fact]
        public async void GivenAzureTableRepository_WhenBatchRemove_ThenProperMethodIsCalled()
        {
            var entities = new List<FirstTableRecordClass>
            {
                new()
                {
                    PartitionKey = "PartitionKey1"
                },
                new()
                {
                    PartitionKey = "PartitionKey2"
                }
            };
            var sut = new AzureTableRepository<FirstTableRecordClass>(_tableClientMock.Object);

            await sut.BatchRemoveAsync(entities);

            _tableClientMock.Verify(x => x.SubmitTransactionAsync(It.IsAny<List<TableTransactionAction>>(), default), Times.Exactly(2));
        }

        [Fact]
        public void GivenAzureTableRepository_WhenQuery_ThenProperMethodIsCalled()
        {
            var query = "testQueryString";
            var pageSize = 1;
            var sut = new AzureTableRepository<FirstTableRecordClass>(_tableClientMock.Object);
            var expectedResult = new List<FirstTableRecordClass>();
            var page = Page<FirstTableRecordClass>.FromValues(expectedResult, continuationToken: null, Mock.Of<Response>());
            var pageable = Pageable<FirstTableRecordClass>.FromPages(new[] { page });
            _tableClientMock.Setup(x => x.Query<FirstTableRecordClass>(query, pageSize, null, default))
                .Returns(pageable);

            var result = sut.Query(query, pageSize);

            Assert.Equal(expectedResult, result);
            _tableClientMock.Verify(x => x.Query<FirstTableRecordClass>(query, pageSize, null, default), Times.Once);
        }

        [Fact]
        public void GivenAzureTableRepository_WhenQueryAsync_ThenProperMethodIsCalled()
        {
            var query = "testQueryString";
            var pageSize = 1;
            var sut = new AzureTableRepository<FirstTableRecordClass>(_tableClientMock.Object);
            var expectedResult = new List<FirstTableRecordClass>();
            var page = Page<FirstTableRecordClass>.FromValues(expectedResult, continuationToken: null, Mock.Of<Response>());
            var pageable = AsyncPageable<FirstTableRecordClass>.FromPages(new[] { page });
            _tableClientMock.Setup(x => x.QueryAsync<FirstTableRecordClass>(query, pageSize, null, default))
                .Returns(pageable);

            var result = sut.QueryAsync(query, pageSize);

            Assert.NotNull(result);
            _tableClientMock.Verify(x => x.QueryAsync<FirstTableRecordClass>(query, pageSize, null, default), Times.Once);
        }

        [Fact]
        public void GivenAzureTableRepository_WhenQueryWithExpressions_ThenProperMethodIsCalled()
        {
            Expression<Func<FirstTableRecordClass, bool>> expression = x => x.PartitionKey == "test";
            var pageSize = 1;
            var sut = new AzureTableRepository<FirstTableRecordClass>(_tableClientMock.Object);
            var expectedResult = new List<FirstTableRecordClass>();
            var page = Page<FirstTableRecordClass>.FromValues(expectedResult, continuationToken: null, Mock.Of<Response>());
            var pageable = Pageable<FirstTableRecordClass>.FromPages(new[] { page });
            _tableClientMock.Setup(x => x.Query<FirstTableRecordClass>(expression, pageSize, null, default))
                .Returns(pageable);

            var result = sut.Query(expression, pageSize);

            Assert.Equal(expectedResult, result);
            _tableClientMock.Verify(x => x.Query<FirstTableRecordClass>(expression, pageSize, null, default), Times.Once);
        }

        [Fact]
        public void GivenAzureTableRepository_WhenQueryAsyncWithExpressions_ThenProperMethodIsCalled()
        {
            Expression<Func<FirstTableRecordClass, bool>> expression = x => x.PartitionKey == "test";
            var pageSize = 1;
            var sut = new AzureTableRepository<FirstTableRecordClass>(_tableClientMock.Object);
            var expectedResult = new List<FirstTableRecordClass>();
            var page = Page<FirstTableRecordClass>.FromValues(expectedResult, continuationToken: null, Mock.Of<Response>());
            var pageable = AsyncPageable<FirstTableRecordClass>.FromPages(new[] { page });
            _tableClientMock.Setup(x => x.QueryAsync<FirstTableRecordClass>(expression, pageSize, null, default))
                .Returns(pageable);

            var result = sut.QueryAsync(expression, pageSize);

            Assert.NotNull(result );
            _tableClientMock.Verify(x => x.QueryAsync<FirstTableRecordClass>(expression, pageSize, null, default), Times.Once);
        }
    }
}