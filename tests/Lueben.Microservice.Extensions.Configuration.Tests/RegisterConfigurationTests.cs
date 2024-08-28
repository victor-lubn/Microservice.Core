using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Lueben.Microservice.Extensions.Configuration.Tests
{
    public class RegisterConfigurationTests
    {
        private readonly ServiceCollection _services;

        public RegisterConfigurationTests()
        {
            _services = new ServiceCollection();
        }

        [Fact]
        public void GivenConfiguration_WhenValueIsSet_ThenItIsAvailableInRegisteredOptions()
        {
            const string propertyValue = "test";
            var json = $"{{\"{nameof(TestOptions)}:{nameof(TestOptions.Property)}\":\"{propertyValue}\"}}";
            _services.AddConfiguration(json).RegisterConfiguration<TestOptions>(nameof(TestOptions));
            var provider = _services.BuildServiceProvider();

            var options = provider.GetService<IOptions<TestOptions>>();

            Assert.NotNull(options);
            Assert.Equal(propertyValue, options.Value.Property);
        }

        [Fact]
        public void GivenConfiguration_WhenValueIsNotSet_ThenItIsNullInRegisteredOptions()
        {
            _services.AddConfiguration("{}").RegisterConfiguration<TestOptions>(nameof(TestOptions));
            var provider = _services.BuildServiceProvider();

            var options = provider.GetService<IOptions<TestOptions>>();

            Assert.NotNull(options);
            Assert.Null(options.Value.Property);
        }
    }
}