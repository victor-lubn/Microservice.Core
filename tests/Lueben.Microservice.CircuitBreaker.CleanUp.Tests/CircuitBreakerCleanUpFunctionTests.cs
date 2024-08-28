using System.Collections.Generic;
using AutoFixture.AutoMoq;
using AutoFixture;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.CircuitBreaker.CleanUp.Tests
{
    public class CircuitBreakerCleanUpFunctionTests
    {
        private readonly IFixture _fixture;

        public CircuitBreakerCleanUpFunctionTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task GivenCleanUpFunction_WhenExecuted_ThenCleanerIsExecutedWithRightParameters()
        {
            var cleanerMock = _fixture.Freeze<Mock<IEntityAzureStorageCleaner>>();

            var optionsMock = _fixture.Freeze<Mock<IOptionsSnapshot<EntityCleanUpOptions>>>();
            optionsMock.Setup(x => x.Value)
                .Returns(new EntityCleanUpOptions()
                {
                    PurgeWithoutAnalyze = true,
                    Ids = new List<string> {"key1"}
                });

            var function = _fixture.Create<CircuitBreakerCleanUpFunction>();

            await function.CircuitBreakerMaintenance(null);

            cleanerMock.Verify(x => x.CleanEntityHistory(
                It.Is<EntityCleanUpOptions>(o => o.EntityName == "durablecircuitbreaker" && o.PurgeWithoutAnalyze && o.Ids.Count == 1)), Times.Once);
        }
    }
}
