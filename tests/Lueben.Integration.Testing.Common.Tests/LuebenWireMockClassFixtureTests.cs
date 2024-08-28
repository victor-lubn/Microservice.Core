using System;
using System.IO;
using Moq;
using WireMock.Settings;
using Xunit;

namespace Lueben.Integration.Testing.Common.Tests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public class LuebenWireMockClassFixtureTests : IDisposable
    {
        private readonly LuebenWireMockClassFixture _classFixture;

        public LuebenWireMockClassFixtureTests()
        {
            _classFixture = new LuebenWireMockClassFixture();
        }

        public void Dispose()
        {
            _classFixture.Dispose();
        }

        [Fact]
        public void GivenLuebenWireMockClassFixture_WhenSettingPactDirectory_ThenDirectoryShouldBeCreated()
        {
            var pactsDir = @"./pacts";

            try
            {
                _classFixture.SetPactsDirectory(pactsDir);

                Assert.True(Directory.Exists(pactsDir));
                Assert.Equal(pactsDir, _classFixture.PactDir);
            }
            finally
            {
                if (Directory.Exists(pactsDir))
                {
                    Directory.Delete(pactsDir, true);
                }
            }
        }

        [Fact]
        public void GivenLuebenWireMockClassFixture_ServerShouldBeInitializer()
        {
            var initializeActionMock = new Mock<Action<WireMockServerSettings>>();

            var server = _classFixture.InitializeServer(initializeActionMock.Object, "Foo");

            Assert.NotNull(server);
            Assert.True(server.IsStarted);
            Assert.Equal("Foo", server.Consumer);

            var server1 = _classFixture.InitializeServer(initializeActionMock.Object);

            Assert.Same(server, server1);
            Assert.Equal("Foo", server1.Consumer);
        }
    }
}
