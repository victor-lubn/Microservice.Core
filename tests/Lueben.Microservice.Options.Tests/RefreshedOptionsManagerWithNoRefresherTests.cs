using Lueben.Microservice.Options.OptionManagers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lueben.Microservice.Options.Tests
{
    public class RefreshedOptionsManagerWithNoRefresherTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GivenRefreshedOptionsManagerWithNoRefresher_GetOptions_ShouldReturnAsExpected(bool useOptions)
        {
            var serviceCollection = new ServiceCollection()
                .AddSingleton<RefreshedOptionsManagerWithNoRefresher>();

            if (useOptions)
            {
                serviceCollection.AddOptions();
            }

            var serviceProvider = serviceCollection.BuildServiceProvider();

            if (useOptions)
            {
                Assert.NotNull(serviceProvider.GetService<RefreshedOptionsManagerWithNoRefresher>().GetOptions<object>());
                return;
            }

            Assert.Null(serviceProvider.GetService<RefreshedOptionsManagerWithNoRefresher>().GetOptions<object>());
        }
    }
}
