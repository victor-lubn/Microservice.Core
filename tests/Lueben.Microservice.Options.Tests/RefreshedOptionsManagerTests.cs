using System;
using System.Threading;
using Lueben.Microservice.Options.OptionManagers;
using Lueben.Microservice.Options.Options;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Lueben.Microservice.Options.Tests
{
    public class RefreshedOptionsManagerTests
    {
        private readonly Mock<IConfigurationRefresher> _refresherMock;
        private readonly Mock<IConfigurationRefresherProvider> _refresherProviderMock;
        private readonly Mock<ILogger<RefreshedOptionsManager>> _loggerMock;

        public RefreshedOptionsManagerTests()
        {
            _refresherMock = new Mock<IConfigurationRefresher>();
            _refresherMock.Setup(x => x.TryRefreshAsync(default(CancellationToken))).ReturnsAsync(true);

            _refresherProviderMock = new Mock<IConfigurationRefresherProvider>();
            _refresherProviderMock.Setup(x => x.Refreshers).Returns(new[] { _refresherMock.Object });

            _loggerMock = new Mock<ILogger<RefreshedOptionsManager>>();
        }

        [Theory]
        [InlineData(true, LogLevel.Information, "Successfully")]
        [InlineData(false, LogLevel.Error, "Unsuccessfully")]
        public void GivenRefreshedOptionsManager_WhenRefreshersExist_ThenShouldTriggerAppConfigRefresh(bool refreshed, LogLevel logLevel, string log)
        {
            _refresherMock.Setup(x => x.TryRefreshAsync(default(CancellationToken))).ReturnsAsync(refreshed);

            var appConfigurationRefreshOptionsMock = new Mock<IOptions<AppConfigurationRefreshOptions>>();
            appConfigurationRefreshOptionsMock.Setup(x => x.Value).Returns(new AppConfigurationRefreshOptions
            {
                DueTime = TimeSpan.FromSeconds(0.5),
                Period = TimeSpan.FromSeconds(1),
            });

            using (var optionsManager = new RefreshedOptionsManager(
                _refresherProviderMock.Object,
                new ServiceCollection().BuildServiceProvider(),
                _loggerMock.Object,
                appConfigurationRefreshOptionsMock.Object))
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                _refresherMock.Verify(x => x.TryRefreshAsync(default(CancellationToken)), Times.Once);

                _loggerMock.Verify(
                    m => m.Log(
                        logLevel,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, _) => v.ToString().StartsWith($"{log} executed configuration refreshing")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }
    }
}
