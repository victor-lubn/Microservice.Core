using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using AutoFixture.AutoMoq;
using Lueben.Integration.Testing.Common.Extensions;
using Lueben.Integration.Testing.WireMock;
using WireMock.Settings;
using Xunit;
using Xunit.Abstractions;

namespace Lueben.Integration.Testing.Common
{
    public abstract class BaseFunctionIntegrationTest<TFunction> : IClassFixture<LuebenWireMockClassFixture>
    {
        [SuppressMessage("Usage", "CA2214:Do not call overridable methods in constructors", Justification = "XUnit uses constructors as a setup before each test. Justified behavior.")]
        public BaseFunctionIntegrationTest(
            LuebenWireMockClassFixture wireMockClassFixture,
            ITestOutputHelper testOutputHelper,
            string consumer)
        {
            WireMockServer = wireMockClassFixture.InitializeServer(
                settings =>
                {
                    settings.WithXUnitOutput(testOutputHelper);
                    ConfigureServer(settings);
                }, 
                consumer);

            WireMockServer.Reset();

            Fixture = new Fixture().Customize(new AutoMoqCustomization());
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            SetupServices();
            Function = this.Fixture.Create<TFunction>();
        }

        protected virtual LuebenWireMockServer WireMockServer { get; set; }

        protected IFixture Fixture { get; set; }

        protected virtual TFunction Function { get; set; }

        protected abstract void SetupServices();

        protected virtual void ConfigureServer(WireMockServerSettings serverSettings)
        {
        }
    }
}
