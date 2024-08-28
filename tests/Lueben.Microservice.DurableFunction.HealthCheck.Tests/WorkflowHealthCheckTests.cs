using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Lueben.Microservice.DurableFunction.HealthCheck.Tests
{
    public class WorkflowHealthCheckTests
    {
        private const int MaxDaysSinceLastUpdated = 1;

        private readonly Mock<ILogger<WorkflowHealthCheck>> _logger;
        private readonly string _instanceId = Guid.NewGuid().ToString();
        private readonly Mock<IDurableClientFactory> _durableClientFactoryMock;
        private readonly Mock<IDurableClient> _durableClientMock;
        private readonly Mock<IOptionsSnapshot<DurableTaskOptions>> _mockTaskOptions;
        private readonly Mock<IOptionsSnapshot<WorkflowHealthCheckOptions>> _mockHealthCheckOptions;

        public WorkflowHealthCheckTests()
        {
            _logger = new Mock<ILogger<WorkflowHealthCheck>>();
            _durableClientFactoryMock = new Mock<IDurableClientFactory>();
            _durableClientMock = new Mock<IDurableClient>();
            _durableClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<DurableClientOptions>()))
                .Returns(_durableClientMock.Object);

            var taskOptions = Options.Create(new DurableTaskOptions { HubName = "hubName" });
            _mockTaskOptions = new Mock<IOptionsSnapshot<DurableTaskOptions>>();
            _mockTaskOptions.Setup(x => x.Value)
                .Returns(taskOptions.Value);

            var healthCheckOptions = Options.Create(new WorkflowHealthCheckOptions
            { MaxDaysSinceLastUpdated = MaxDaysSinceLastUpdated });
            _mockHealthCheckOptions = new Mock<IOptionsSnapshot<WorkflowHealthCheckOptions>>();
            _mockHealthCheckOptions.Setup(x => x.Value)
                .Returns(healthCheckOptions.Value);
        }

        [Fact]
        public async Task GivenWorkflowHealthCheck_WhenItFails_ThenStatusIsUnhealthy()
        {
            _durableClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<DurableClientOptions>()))
                .Throws<Exception>();
            var healthCheck = new WorkflowHealthCheck(_durableClientFactoryMock.Object, _mockHealthCheckOptions.Object,
                _mockTaskOptions.Object, _logger.Object, _instanceId);

            var status = await healthCheck.CheckHealthAsync(null);

            _durableClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<DurableClientOptions>()), Times.Once);
            Assert.Equal(HealthStatus.Unhealthy, status.Status);
        }

        [Fact]
        public async Task GivenWorkflowHealthCheck_WhenDurableClientRequestFails_ThenStatusIsUnhealthy()
        {
            _durableClientMock.Setup(x => x.GetStatusAsync(_instanceId, false, false, true))
                .Throws<Exception>();
            var healthCheck = new WorkflowHealthCheck(_durableClientFactoryMock.Object, _mockHealthCheckOptions.Object,
                _mockTaskOptions.Object, _logger.Object, _instanceId);

            var status = await healthCheck.CheckHealthAsync(null);

            _durableClientMock.Verify(x => x.GetStatusAsync(_instanceId, false, false, true), Times.Once);
            Assert.Equal(HealthStatus.Unhealthy, status.Status);
        }

        [Theory]
        [InlineData(OrchestrationRuntimeStatus.Canceled, 0, HealthStatus.Healthy)]
        [InlineData(OrchestrationRuntimeStatus.Completed, 0, HealthStatus.Healthy)]
        [InlineData(OrchestrationRuntimeStatus.Terminated, 0, HealthStatus.Healthy)]
        [InlineData(OrchestrationRuntimeStatus.Unknown, 0, HealthStatus.Healthy)]
        [InlineData(OrchestrationRuntimeStatus.ContinuedAsNew, 0, HealthStatus.Healthy)]
        [InlineData(OrchestrationRuntimeStatus.Failed, 0, HealthStatus.Unhealthy)]
        [InlineData(OrchestrationRuntimeStatus.Pending, 0, HealthStatus.Healthy)]
        [InlineData(OrchestrationRuntimeStatus.Running, 0, HealthStatus.Healthy)]
        [InlineData(OrchestrationRuntimeStatus.Pending, MaxDaysSinceLastUpdated, HealthStatus.Healthy)]
        [InlineData(OrchestrationRuntimeStatus.Running, MaxDaysSinceLastUpdated, HealthStatus.Healthy)]
        [InlineData(OrchestrationRuntimeStatus.Pending, MaxDaysSinceLastUpdated + 1, HealthStatus.Unhealthy)]
        [InlineData(OrchestrationRuntimeStatus.Running, MaxDaysSinceLastUpdated + 1, HealthStatus.Unhealthy)]
        public async Task GivenWorkflowHealthCheck_WhenOneOfTheStatuses_ThenStatusIsAsExpected(
            OrchestrationRuntimeStatus inputStatus, int lastUpdatedDaysAgo, HealthStatus healthStatus)
        {
            var orchestrationStatus = new DurableOrchestrationStatus
            {
                RuntimeStatus = inputStatus,
                LastUpdatedTime = DateTime.Now.AddDays(-lastUpdatedDaysAgo).AddSeconds(10)
            };
            _durableClientMock.Setup(x => x.GetStatusAsync(_instanceId, false, false, true))
                .Returns(Task.FromResult(orchestrationStatus));
            var healthCheck = new WorkflowHealthCheck(_durableClientFactoryMock.Object, _mockHealthCheckOptions.Object,
                _mockTaskOptions.Object, _logger.Object, _instanceId);

            var status = await healthCheck.CheckHealthAsync(null);

            Assert.Equal(healthStatus, status.Status);
        }

        [Fact]
        public async Task GivenWorkflowHealthCheck_WhenIsLongRunningButItIsEntityOrchestration_ThenStatusIsStillHealthy()
        {
            _durableClientMock.Setup(x => x.ListInstancesAsync(It.Is<OrchestrationStatusQueryCondition>(c => c.ContinuationToken == null),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new OrchestrationStatusQueryResult
                {
                    DurableOrchestrationState = new[]
                    {
                        new DurableOrchestrationStatus
                        {
                            InstanceId = "@durablecircuitbreaker@ApiClient",
                            RuntimeStatus = OrchestrationRuntimeStatus.Running,
                            LastUpdatedTime = DateTime.Now.AddDays(-(MaxDaysSinceLastUpdated + 1)).AddSeconds(10)
                        }
                    }
                }));
            var healthCheck = new WorkflowHealthCheck(_durableClientFactoryMock.Object, _mockHealthCheckOptions.Object, _mockTaskOptions.Object, _logger.Object, null);

            var result = await healthCheck.CheckHealthAsync(null);

            _durableClientMock.Verify(x => x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

            Assert.Equal(HealthStatus.Healthy, result.Status);
        }

        [Fact]
        public async Task GivenWorkflowHealthCheck_WhenDurableClientReturnsNull_ThenStatusIsHealthy()
        {
            _durableClientMock.Setup(x => x.GetStatusAsync(_instanceId, false, false, true))
                .Returns(Task.FromResult((DurableOrchestrationStatus)null));
            var healthCheck = new WorkflowHealthCheck(_durableClientFactoryMock.Object, _mockHealthCheckOptions.Object,
                _mockTaskOptions.Object, _logger.Object, _instanceId);

            var status = await healthCheck.CheckHealthAsync(null);

            _durableClientMock.Verify(x => x.GetStatusAsync(_instanceId, false, false, true), Times.Once);
            Assert.Equal(HealthStatus.Healthy, status.Status);
        }

        [Fact]
        public async Task GivenWorkflowHealthCheck_WhenNoInstanceId_ThenCorrectSearchParametersAreUsed()
        {
            _durableClientMock.Setup(x =>
                    x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new OrchestrationStatusQueryResult
                { DurableOrchestrationState = null, ContinuationToken = null }));
            var healthCheck = new WorkflowHealthCheck(_durableClientFactoryMock.Object, _mockHealthCheckOptions.Object,
                _mockTaskOptions.Object, _logger.Object, null);

            var status = await healthCheck.CheckHealthAsync(null);

            _durableClientMock.Verify(x => x.ListInstancesAsync(It.Is<OrchestrationStatusQueryCondition>(c =>
                    c.PageSize == _mockHealthCheckOptions.Object.Value.HistoryPageSize
                    && c.RuntimeStatus.Contains(OrchestrationRuntimeStatus.Pending)
                    && c.RuntimeStatus.Contains(OrchestrationRuntimeStatus.Running)
                ),
                It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(HealthStatus.Healthy, status.Status);
        }

        [Fact]
        public async Task GivenWorkflowHealthCheck_WhenHistoryIsEmpty_ThenStatusIsHealthy()
        {
            _durableClientMock.Setup(x =>
                    x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new OrchestrationStatusQueryResult
                { DurableOrchestrationState = null, ContinuationToken = null }));
            var healthCheck = new WorkflowHealthCheck(_durableClientFactoryMock.Object, _mockHealthCheckOptions.Object,
                _mockTaskOptions.Object, _logger.Object, null);

            var status = await healthCheck.CheckHealthAsync(null);

            _durableClientMock.Verify(x => x.GetStatusAsync(_instanceId, false, false, true), Times.Never);
            _durableClientMock.Verify(
                x => x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.Equal(HealthStatus.Healthy, status.Status);
        }

        [Fact]
        public async Task GivenWorkflowHealthCheck_WhenHistoryHas2PagesWithoutLongRunningInstances_ThenBothPagesAreRetrieved()
        {
            _durableClientMock.Setup(x => x.ListInstancesAsync(It.Is<OrchestrationStatusQueryCondition>(c => c.ContinuationToken == null),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new OrchestrationStatusQueryResult
                {
                    DurableOrchestrationState = new[]
                    {
                        new DurableOrchestrationStatus()
                        {
                            CreatedTime = DateTime.Now.AddSeconds(-1),
                            LastUpdatedTime = DateTime.Now
                        }
                    },
                    ContinuationToken = "1"
                }));
            _durableClientMock.Setup(x => x.ListInstancesAsync(It.Is<OrchestrationStatusQueryCondition>(c => c.ContinuationToken == "1"),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new OrchestrationStatusQueryResult
                { DurableOrchestrationState = new[] { new DurableOrchestrationStatus() }, ContinuationToken = null }));

            var healthCheck = new WorkflowHealthCheck(_durableClientFactoryMock.Object, _mockHealthCheckOptions.Object, _mockTaskOptions.Object, _logger.Object, null);

            await healthCheck.CheckHealthAsync(null);

            _durableClientMock.Verify(x => x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GivenWorkflowHealthCheck_WhenFirstPageReturnsLongRunningInstance_ThenSecondPageIsNotRetrieved()
        {
            _durableClientMock.Setup(x => x.ListInstancesAsync(It.Is<OrchestrationStatusQueryCondition>(c => c.ContinuationToken == null),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new OrchestrationStatusQueryResult
                {
                    DurableOrchestrationState = new[]
                    {
                        new DurableOrchestrationStatus
                        {
                            CreatedTime = DateTime.Now.AddDays(-3 * _mockHealthCheckOptions.Object.Value.MaxDaysSinceLastUpdated),
                            LastUpdatedTime = DateTime.Now.AddDays(-2 * _mockHealthCheckOptions.Object.Value.MaxDaysSinceLastUpdated)
                        }
                    },
                    ContinuationToken = "1"
                }));
            _durableClientMock.Setup(x =>
                    x.ListInstancesAsync(It.Is<OrchestrationStatusQueryCondition>(c => c.ContinuationToken == "1"),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new OrchestrationStatusQueryResult
                { DurableOrchestrationState = new[] { new DurableOrchestrationStatus() }, ContinuationToken = null }));

            var healthCheck = new WorkflowHealthCheck(_durableClientFactoryMock.Object, _mockHealthCheckOptions.Object, _mockTaskOptions.Object, _logger.Object, null);

            var status = await healthCheck.CheckHealthAsync(null);

            _durableClientMock.Verify(x => x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            Assert.Equal(HealthStatus.Unhealthy, status.Status);
        }

        [Theory]
        [InlineData(0, 0, HealthStatus.Healthy)]
        [InlineData(MaxDaysSinceLastUpdated, 0, HealthStatus.Healthy)]
        [InlineData(2 * MaxDaysSinceLastUpdated, 2 * MaxDaysSinceLastUpdated, HealthStatus.Unhealthy)]
        [InlineData(2 * MaxDaysSinceLastUpdated, 0, HealthStatus.Unhealthy)]
        public async Task GivenWorkflowHealthCheck_WhenCreatedAndLastUpdatedDateAreSet_ThenStatusIsAsExpected(int createdAgo, int updatedAgo, HealthStatus expectedStatus)
        {
            _durableClientMock.Setup(x => x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new OrchestrationStatusQueryResult
                {
                    DurableOrchestrationState = new[]
                    {
                        new DurableOrchestrationStatus
                        {
                            CreatedTime = DateTime.Now.AddDays(-createdAgo).AddSeconds(5),
                            LastUpdatedTime = DateTime.Now.AddDays(-updatedAgo)
                        }
                    }
                }));
            var healthCheck = new WorkflowHealthCheck(_durableClientFactoryMock.Object, _mockHealthCheckOptions.Object, _mockTaskOptions.Object, _logger.Object, null);

            var status = await healthCheck.CheckHealthAsync(null);

            Assert.Equal(expectedStatus, status.Status);
        }
    }
}