using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Azure;
using Moq;
using Xunit;

namespace Lueben.Microservice.CircuitBreaker.CleanUp.Tests
{
    public class EntityAzureStorageCleanerTests
    {
        private const string CbEntityName = "durablecircuitbreaker";
        private readonly IFixture _fixture;
        private readonly Mock<IDurableClient> _durableClientMock;
        private readonly Mock<TableClient> _tableClientMock;

        public EntityAzureStorageCleanerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _tableClientMock = new Mock<TableClient>();

            var tableServiceClientMock = new Mock<TableServiceClient>();
            tableServiceClientMock.Setup(x => x.GetTableClient(It.IsAny<string>())).Returns(_tableClientMock.Object);

            var tableClintFactoryMock = _fixture.Freeze<Mock<IAzureClientFactory<TableServiceClient>>>();
            tableClintFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(tableServiceClientMock.Object);

            var durableClientFactoryMock = _fixture.Freeze<Mock<IDurableClientFactory>>();
            _durableClientMock = new Mock<IDurableClient>();
            durableClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<DurableClientOptions>()))
                .Returns(_durableClientMock.Object);
        }

        [Fact]
        public async Task GivenEntityAzureStorageCleaner_WhenExecuted_ThenListEntitiesAsyncExecutedForDurableCircuitBreakerEntities()
        {
            var cleaner = _fixture.Create<EntityAzureStorageCleaner>();

            await cleaner.CleanEntityHistory(new EntityCleanUpOptions {EntityName = CbEntityName });

            _durableClientMock.Verify(x => x.ListEntitiesAsync(It.Is<EntityQuery>(c => c.EntityName == CbEntityName), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GivenEntityAzureStorageCleaner_WhenExecutedWithPurgeWithoutAnalyze_ThenAllCircuitBreakersArePurged()
        {
            var cleaner = _fixture.Create<EntityAzureStorageCleaner>();

            var entityStatusMock = new Mock<DurableEntityStatus>
            {
                Object = { EntityId = new EntityId(CbEntityName, "key1") }
            };
            var entitiesResultMock = new Mock<EntityQueryResult>
            {
                Object = { Entities = new[] { entityStatusMock.Object } }
            };

            _durableClientMock.Setup(x => x.ListEntitiesAsync(It.IsAny<EntityQuery>(), CancellationToken.None))
                .Returns(Task.FromResult(entitiesResultMock.Object));

            await cleaner.CleanEntityHistory(new EntityCleanUpOptions { EntityName = CbEntityName, PurgeWithoutAnalyze = true });

            _durableClientMock.Verify(x => x.PurgeInstanceHistoryAsync(entityStatusMock.Object.EntityId.ToString()), Times.Once);
        }

        [Fact]
        public async Task GivenEntityAzureStorageCleaner_WhenExecutedWithoutPurgeWithoutAnalyzeAndBlobExists_ThenAllCircuitBreakersArePurged()
        {
            var cleaner = _fixture.Create<EntityAzureStorageCleaner>();

            const string entityKeyMock = "key1";
            var entitiesResultMock = CreateResultFromEntities(new[] { entityKeyMock });
            var mockInstanceId = GetInstanceId(entityKeyMock);

            var historyDictionary = new Dictionary<string, object> { {"CorrelationBlobName", "blobPath"} };
            var expectedResult = new List<TableEntity> { new(historyDictionary) };
            var page = Page<TableEntity>.FromValues(expectedResult, continuationToken: null, Mock.Of<Response>());
            var pageable = AsyncPageable<TableEntity>.FromPages(new[] { page });
            _tableClientMock.Setup(x => x.QueryAsync<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), default))
                .Returns(pageable);

            _durableClientMock.Setup(x => x.ListEntitiesAsync(It.IsAny<EntityQuery>(), CancellationToken.None))
                .Returns(Task.FromResult(entitiesResultMock));

            await cleaner.CleanEntityHistory(new EntityCleanUpOptions { EntityName = CbEntityName, PurgeWithoutAnalyze = false });

            _durableClientMock.Verify(x => x.PurgeInstanceHistoryAsync(mockInstanceId), Times.Once);
        }

        [Fact]
        public async Task GivenEntityAzureStorageCleaner_WhenExecutedWithoutPurgeWithoutAnalyzeAndBlobNotExists_ThenAllCircuitBreakersAreNotPurged()
        {
            var cleaner = _fixture.Create<EntityAzureStorageCleaner>();

            const string entityKeyMock = "key1";
            var entitiesResultMock = CreateResultFromEntities(new [] { entityKeyMock });
            var mockInstanceId = GetInstanceId(entityKeyMock);

            _durableClientMock.Setup(x => x.ListEntitiesAsync(It.IsAny<EntityQuery>(), CancellationToken.None))
                .Returns(Task.FromResult(entitiesResultMock));

            var historyDictionary = new Dictionary<string, object> { { "CorrelationBlobName", string.Empty } };
            var expectedResult = new List<TableEntity> { new(historyDictionary) };
            var page = Page<TableEntity>.FromValues(expectedResult, continuationToken: null, Mock.Of<Response>());
            var pageable = AsyncPageable<TableEntity>.FromPages(new[] { page });
            _tableClientMock.Setup(x => x.QueryAsync<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), default))
                .Returns(pageable);

            await cleaner.CleanEntityHistory(new EntityCleanUpOptions { EntityName = CbEntityName, PurgeWithoutAnalyze = false });

            _durableClientMock.Verify(x => x.PurgeInstanceHistoryAsync(mockInstanceId), Times.Never);
        }

        [Fact]
        public async Task GivenEntityAzureStorageCleaner_WhenExecutedWithIdsFilter_ThenOnlyFilteredCBArePurged()
        {
            var cleaner = _fixture.Create<EntityAzureStorageCleaner>();

            const string entityKeyMock1 = "key1";
            const string entityKeyMock2 = "key2";
            var entitiesResultMock = CreateResultFromEntities(new[] { entityKeyMock1, entityKeyMock2 });

            _durableClientMock.Setup(x => x.ListEntitiesAsync(It.IsAny<EntityQuery>(), CancellationToken.None))
                .Returns(Task.FromResult(entitiesResultMock));

            await cleaner.CleanEntityHistory(new EntityCleanUpOptions { EntityName = CbEntityName, PurgeWithoutAnalyze = true, Ids = new List<string> {entityKeyMock1}});

            _durableClientMock.Verify(x => x.PurgeInstanceHistoryAsync(It.IsAny<string>()), Times.Once);
        }

        private static EntityQueryResult CreateResultFromEntities(IEnumerable<string> keys)
        {
            var entities = keys.Select(key => new Mock<DurableEntityStatus> { Object = { EntityId = new EntityId(CbEntityName, key) } })
                .Select(entityStatusMock => entityStatusMock.Object);

            var entitiesResultMock = new Mock<EntityQueryResult> { Object = { Entities = entities } };

            return entitiesResultMock.Object;
        }

        private static string GetInstanceId(string key)
        {
            return new EntityId(CbEntityName, key).ToString();
        }
    }
}
