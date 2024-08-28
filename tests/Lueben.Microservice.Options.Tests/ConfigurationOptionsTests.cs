using Azure.Core;
using Lueben.Microservice.Options.Extensions;
using Lueben.Microservice.Options.OptionManagers;
using Lueben.Microservice.Options.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Lueben.Microservice.Options.Tests
{
    public class ConfigurationOptionsTests
    {
        [Theory]
        [InlineData("", "Options", "Options")]
        [InlineData("Hodens.Microservice", "Options", "Hodens.Microservice:Options")]
        public void GetKey_WhenEmptyPrefix_ThenNameIsReturned(string prefix, string name, string expected)
        {
            var actualResult = ConfigurationBuilderExtensions.GetKey(prefix, name);
            Assert.Equal(expected, actualResult);
        }

        [Fact]
        public void GivenAddLuebenAzureAppConfiguration_GivenNoConnectionOptions_ShouldReturnConfigurationBuilderWithNoSources()
        {
            var mockTokenCredential = new Mock<TokenCredential>();
            var fakeTokenCredential = mockTokenCredential.Object;

            var fakeConfigurationBuilder = new ConfigurationBuilder();

            var actualResult = fakeConfigurationBuilder.AddLuebenAzureAppConfiguration(fakeTokenCredential);

            Assert.Equal(0, actualResult.Sources.Count);
        }

        [Fact]
        public void GivenAddLuebenAzureAppConfiguration_GivenOneConnectionOptions_ShouldReturnConfigurationBuilderWithOneSource_ConnectionThroughConnectionString()
        {
            var mockTokenCredential = new Mock<TokenCredential>();
            var fakeTokenCredential = mockTokenCredential.Object;

            var fakeConfigurationBuilder = new ConfigurationBuilder();

            using (new EnvironmentVariableContext("ConnectionStrings:AzureAppConfiguration", "configuration"))
            {
                var actualResult = fakeConfigurationBuilder.AddLuebenAzureAppConfiguration(fakeTokenCredential);

                Assert.Equal(1, actualResult.Sources.Count);
            }
        }

        [Fact]
        public void GivenAddLuebenAzureAppConfiguration_GivenOneConnectionOptions_ShouldReturnConfigurationBuilderWithOneSource_ConnectionThroughEndpoint()
        {
            var mockTokenCredential = new Mock<TokenCredential>();
            var fakeTokenCredential = mockTokenCredential.Object;

            var fakeConfigurationBuilder = new ConfigurationBuilder();

            using (new EnvironmentVariableContext("AzureAppConfigurationEndpoint", "urlOfEndpoint"))
            {
                var actualResult = fakeConfigurationBuilder.AddLuebenAzureAppConfiguration(fakeTokenCredential);

                Assert.Equal(1, actualResult.Sources.Count);
            }
        }

        [Fact]
        public void GivenAddLuebenAzureAppConfigurationWithNoRefresher_ThenShouldAddRefreshedOptionsManagerWithNoRefresherToServiceCollection()
        {
            var services = new ServiceCollection().AddLuebenAzureAppConfigurationWithNoRefresher();

            var serviceProvider = services.BuildServiceProvider();

            Assert.IsType<RefreshedOptionsManagerWithNoRefresher>(serviceProvider.GetRequiredService<IRefreshedOptionsManager>());
        }

        [Fact]
        public void GivenAddLuebenAzureAppConfigurationWithNoRefresher_ThenShouldAddRefreshedOptionsManagerToServiceCollection()
        {
            var services = new ServiceCollection()
                .AddLuebenAzureAppConfiguration()
                .AddLogging()
                .AddSingleton(new Mock<IConfiguration>().Object)
                .AddSingleton(new Mock<IConfigurationRefresherProvider>().Object);

            var serviceProvider = services.BuildServiceProvider();

            Assert.IsType<RefreshedOptionsManager>(serviceProvider.GetRequiredService<IRefreshedOptionsManager>());
        }
    }
}
