using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Lueben.Microservice.Extensions.Configuration.Tests
{
    public class RegisterNamedOptionsTests
    {
        private readonly ServiceCollection _services;

        public RegisterNamedOptionsTests()
        {
            _services = new ServiceCollection();
        }

        private const string PropertyValue = "PropertyValue";

        [Fact]
        public void GivenConfiguration_WhenNamedOptionsAreNotSetAtAll_ThenOptionsAreNull()
        {
            AddConfiguration(_services, null).RegisterNamedOptions<TestOptions>();
            var provider = _services.BuildServiceProvider();

            var options = provider.GetService<IOptionsSnapshot<TestOptions>>();

            Assert.Null(options);
        }

        [Fact]
        public void GivenConfiguration_WhenNamedOptionExists_ThenValueIsRead()
        {
            var mockNamed = "Option1".ToLower();
            AddConfiguration(_services, mockNamed).RegisterNamedOptions<TestOptions>();
            var provider = _services.BuildServiceProvider();

            var options = provider.GetService<IOptionsSnapshot<TestOptions>>();

            Assert.NotNull(options);
            Assert.NotNull(options.Get(mockNamed));
            Assert.Equal(PropertyValue, options.Get(mockNamed).Property);
        }

        [Fact]
        public void GivenConfiguration_WhenNamedOptionSetButNoSettingsForSpecificName_ThenValueIsNull()
        {
            var mockNamed = "Option1".ToLower();
            AddConfiguration(_services, mockNamed).RegisterNamedOptions<TestOptions>();
            var provider = _services.BuildServiceProvider();

            var options = provider.GetService<IOptionsSnapshot<TestOptions>>();

            Assert.NotNull(options);
            Assert.NotNull(options.Get(mockNamed.ToUpper()));
            Assert.Null(options.Get(mockNamed.ToUpper()).Property);
        }

        private static IServiceCollection AddConfiguration(IServiceCollection services, string settingName)
        {
            var json = settingName == null ? "{}" : $"{{\"{nameof(TestOptions)}:{settingName}:{nameof(TestOptions.Property)}\":\"{PropertyValue}\"}}";
            return services.AddConfiguration(json).RegisterNamedOptions<TestOptions>();
        }
    }
}