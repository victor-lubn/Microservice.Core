using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;
using Moq;

namespace Lueben.ApplicationInsights.Tests
{
    public class ApplicationTelemetryInitializerTests
    {
        private readonly Mock<IOptions<ApplicationLogOptions>> _optionsMock;

        public ApplicationTelemetryInitializerTests()
        {
            _optionsMock = new Mock<IOptions<ApplicationLogOptions>>();
        }

        [Fact]
        public void GivenApplicationTelemetryInitializer_WhenIntitializeTelemetry_ThenPassedOptionsAreAddedToTelemetry()
        {
            var options = new ApplicationLogOptions
            {
                Application = "TestApplication",
                ApplicationType = "API",
                Area = string.Empty
            };

            _optionsMock.Setup(x => x.Value).Returns(options);

            var initializer = new ApplicationTelemetryInitializer(_optionsMock.Object);
            
            var telemetry = new TestTelemetry
            {
                Properties = new Dictionary<string, string>()
            };

            initializer.Initialize(telemetry);

            Assert.Contains(Constants.CompanyPrefix + Constants.Separator + ScopeKeys.ApplicationTypeKey, telemetry.Properties.Keys);
            Assert.Contains(Constants.CompanyPrefix + Constants.Separator + ScopeKeys.ApplicationKey, telemetry.Properties.Keys);
            Assert.DoesNotContain(Constants.CompanyPrefix + Constants.Separator + ScopeKeys.AreaKey, telemetry.Properties.Keys);
        }


        [Fact]
        public void GivenApplicationTelemetryInitializer_WhenIntitializeTelemetryAndPropertiesAreNull_ThenNoOptionsAreAddedToTelemetry()
        {
            var options = new ApplicationLogOptions
            {
                Application = "TestApplication",
                ApplicationType = "API",
                Area = string.Empty
            };

            _optionsMock.Setup(x => x.Value).Returns(options);

            var initializer = new ApplicationTelemetryInitializer(_optionsMock.Object);

            var telemetry = new TestTelemetry();

            initializer.Initialize(telemetry);

            Assert.Null(telemetry.Properties);
        }
    }

    public class TestTelemetry: ITelemetry, ISupportProperties
    {
        public void Sanitize()
        {
            throw new NotImplementedException();
        }

        public ITelemetry DeepClone()
        {
            throw new NotImplementedException();
        }

        public void SerializeData(ISerializationWriter serializationWriter)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset Timestamp { get; set; }

        public TelemetryContext Context { get; } = new TelemetryContext();

        public IExtension? Extension { get; set; }

        public string Sequence { get; set; } = string.Empty;

        public IDictionary<string, string>? Properties { get; set; }
    }
}