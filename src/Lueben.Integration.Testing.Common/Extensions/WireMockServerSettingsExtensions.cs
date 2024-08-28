using System.Diagnostics.CodeAnalysis;
using WireMock.Net.Xunit;
using WireMock.Settings;
using Xunit.Abstractions;

namespace Lueben.Integration.Testing.Common.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class WireMockServerSettingsExtensions
    {
        public static WireMockServerSettings WithXUnitOutput(this WireMockServerSettings wiremockServerSettings, ITestOutputHelper testOutputHelper)
        {
            wiremockServerSettings.Logger = new TestOutputHelperWireMockLogger(testOutputHelper);
            return wiremockServerSettings;
        }
    }
}
