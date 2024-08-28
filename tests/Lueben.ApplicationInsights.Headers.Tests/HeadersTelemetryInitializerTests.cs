using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Lueben.ApplicationInsights.Headers.Tests
{
    public class HeadersTelemetryInitializerTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;

        public HeadersTelemetryInitializerTests()
        {
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
        }

        [Fact]
        public void GivenHeadersTelemetryInitializer_WhenInitializedAndAccessorIsNotPassed_ThenArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new HeadersTelemetryInitializer(null, null));
        }

        [Fact]
        public void GivenHeadersTelemetryInitializer_WhenInitializedAndHeadersAreNotPassed_ThenArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new HeadersTelemetryInitializer(_httpContextAccessor.Object, null));
        }

        [Fact]
        public void GivenInitialize_WhenNoHeaders_ThenNoPropertiesToTelemetryAdded()
        {
            var telemetry = new TestTelemetry();
            var initializer = new HeadersTelemetryInitializer(_httpContextAccessor.Object, new List<string>());
            
            initializer.Initialize(telemetry);

            Assert.Null(telemetry.Properties);
        }

        [Fact]
        public void GivenInitialize_WhenNotRequestTelemetry_ThenNoPropertiesToTelemetryAdded()
        {
            var telemetry = new TestTelemetry();
            var initializer = new HeadersTelemetryInitializer(_httpContextAccessor.Object, new List<string>());

            initializer.Initialize(telemetry);

            Assert.Null(telemetry.Properties);
        }

        [Fact]
        public void GivenInitialize_WhenTelemetryPropertiesAreNull_ThenNoPropertiesToTelemetryAdded()
        {
            var expectedHeaderName = "header";
            var expectedHeaderValue = "value";
            var telemetry = new RequestTelemetry();
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(expectedHeaderName, expectedHeaderValue);
            context.Response.Headers.Clear();
            _httpContextAccessor.Setup(x => x.HttpContext).Returns(context);
            var initializer = new HeadersTelemetryInitializer(_httpContextAccessor.Object, new List<string> { expectedHeaderName });

            initializer.Initialize(telemetry);

            Assert.Single(telemetry.Properties);
        }
    }

    public class TestTelemetry : ITelemetry, ISupportProperties
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