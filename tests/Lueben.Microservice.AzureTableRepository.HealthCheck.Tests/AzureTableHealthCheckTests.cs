using Azure.Data.Tables;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;

namespace Lueben.Microservice.AzureTableRepository.HealthCheck.Tests
{
    public class AzureTableHealthCheckTests
    {
        private readonly Mock<IAzureClientFactory<TableServiceClient>> _factoryMock;
        private readonly AzureTableHealthCheck<TestTableEntity> _helthCheck;

        public AzureTableHealthCheckTests()
        {
            _factoryMock = new Mock<IAzureClientFactory<TableServiceClient>>();
            var options = new AzureTableRepositoryOptions();
            _helthCheck = new AzureTableHealthCheck<TestTableEntity>(_factoryMock.Object, options);
        }

        [Fact]
        public async void GivenCheckHealthAsync_WhenNoExceptionsOnAttemptToGetAccessToTable_ThenHelthyStatusIsReturned()
        {
            const string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
            var tableServiceClientMock = new Mock<TableServiceClient>(connectionString);
            _factoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(tableServiceClientMock.Object);

            var result = await _helthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.Equal(HealthStatus.Healthy, result.Status);
        }

        [Fact]
        public async void GivenCheckHealthAsync_WhenExceptionsOnAttemptToGetAccessToTable_ThenNotHelthyStatusIsReturned()
        {
            const string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
            var tableServiceClientMock = new Mock<TableServiceClient>(connectionString);
            _factoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(tableServiceClientMock.Object);
            tableServiceClientMock.Setup(x => x.CreateTableIfNotExistsAsync(It.IsAny<string>(), default))
                .ThrowsAsync(new Exception());
            var healthCheckMock = new Mock<IHealthCheck>();
            var context = new HealthCheckContext
            {
                Registration = new HealthCheckRegistration("testName", healthCheckMock.Object, HealthStatus.Unhealthy, null, null)
            };

            var result = await _helthCheck.CheckHealthAsync(context);

            Assert.Equal(HealthStatus.Unhealthy, result.Status);
        }
    }

    public class TestTableEntity
    {
    }
}