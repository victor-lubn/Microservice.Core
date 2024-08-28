using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Extensions.Configuration;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Options;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;

namespace Lueben.Microservice.CircuitBreaker.Tests
{
    public class CircuitBreakerStateCheckerTests
    {
        private readonly Mock<IDurableClientFactory> _factoryMock;
        private readonly Mock<IDurableCircuitBreakerClient> _clientMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IOptions<DurableTaskOptions>> _optionsMock;
        private readonly Mock<ILogger<CircuitBreakerStateChecker>> _loggerMock;
        private readonly Mock<IDurableClient> _orchestrationClient;

        public CircuitBreakerStateCheckerTests()
        {
            _factoryMock = new Mock<IDurableClientFactory>();
            _clientMock = new Mock<IDurableCircuitBreakerClient>();
            _configurationMock = new Mock<IConfiguration>();
            _optionsMock = new Mock<IOptions<DurableTaskOptions>>();
            _optionsMock.Setup(x => x.Value).Returns(new DurableTaskOptions());
            _loggerMock = new Mock<ILogger<CircuitBreakerStateChecker>>();
            _orchestrationClient = new Mock<IDurableClient>();
            _factoryMock.Setup(x => x.CreateClient(It.IsAny<DurableClientOptions>()))
                .Returns(_orchestrationClient.Object);
        }

        [Fact]
        public void GivenCircuitBreakerStateChecker_WhenLoggerIsNotPassed_ThenArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new CircuitBreakerStateChecker(_factoryMock.Object,
                _clientMock.Object, null, _configurationMock.Object, _optionsMock.Object));
        }

        [Fact]
        public void GivenCircuitBreakerStateChecker_WhenConfigurationIsNotPassed_ThenArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new CircuitBreakerStateChecker(_factoryMock.Object,
                _clientMock.Object, _loggerMock.Object, null, _optionsMock.Object));
        }

        [Fact]
        public void GivenCircuitBreakerStateChecker_WhenCircuitBreakerClientIsNotPassed_ThenArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new CircuitBreakerStateChecker(_factoryMock.Object,
                null, _loggerMock.Object, _configurationMock.Object, _optionsMock.Object));
        }

        [Fact]
        public async Task GivenIsCircuitBreakerInOpenState_WhenCalled_ThenCircuitBreakerStateIsChecked()
        {
            var checker = new CircuitBreakerStateChecker(_factoryMock.Object, _clientMock.Object, _loggerMock.Object, _configurationMock.Object, _optionsMock.Object);
            var cbId1 = "testId1";
            var cbId2 = "testId2";
            _clientMock.Setup(x =>
                x.IsExecutionPermitted(cbId1, _loggerMock.Object, _orchestrationClient.Object,
                    _configurationMock.Object)).ReturnsAsync(true);
            _clientMock.Setup(x =>
                x.IsExecutionPermitted(cbId2, _loggerMock.Object, _orchestrationClient.Object,
                    _configurationMock.Object)).ReturnsAsync(false);

            var result = await checker.IsCircuitBreakerInOpenState(new List<string> { cbId1, cbId2  });

            Assert.True(result);
            _clientMock.Verify(x =>
                x.IsExecutionPermitted(It.IsAny<string>(), _loggerMock.Object, _orchestrationClient.Object,
                    _configurationMock.Object), Times.Exactly(2));
        }

        [Fact]
        public async Task GivenIsCircuitBreakerInOpenState_WhenCalledWithoutParams_ThenCircuitBreakerStateIsCheckedForPredefinedCBs()
        {
            var cbId = "testId";
            var checker = new CircuitBreakerStateChecker(_factoryMock.Object, _clientMock.Object, _loggerMock.Object, _configurationMock.Object, _optionsMock.Object, new List<string> { cbId });
            
            _clientMock.Setup(x =>
                x.IsExecutionPermitted(cbId, _loggerMock.Object, _orchestrationClient.Object,
                    _configurationMock.Object)).ReturnsAsync(true);

            var result = await checker.IsCircuitBreakerInOpenState();

            Assert.False(result);
            _clientMock.Verify(x =>
                x.IsExecutionPermitted(It.IsAny<string>(), _loggerMock.Object, _orchestrationClient.Object,
                    _configurationMock.Object), Times.Once);
        }
    }
}
