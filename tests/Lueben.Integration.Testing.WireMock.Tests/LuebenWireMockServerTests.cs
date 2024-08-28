using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Lueben.Integration.Testing.WireMock.Providers;
using Newtonsoft.Json;
using WireMock.Pact.Models.V2;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Settings;
using Xunit;

namespace Lueben.Integration.Testing.WireMock.Tests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public class LuebenWireMockServerTests : IDisposable
    {
        private readonly IFixture _fixture;
        private readonly LuebenWireMockServer _server;

        public LuebenWireMockServerTests()
        {
            _fixture= new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _server = LuebenWireMockServer.Start(new WireMockServerSettings());
        }

        [Fact]
        public void GivenLuebenWireMockServer_WhenAddingConsumerAndCustomer_ThenShouldBeUpdated()
        {
            _server.WithConsumer("foo")
                .WithProvider("bar");

            Assert.Equal("foo", _server.Consumer);
            Assert.Equal("bar", _server.Provider);
        }

        [Fact]
        public void GivenLuebenWireMockServer_WhenCallingGivenMethod_TheShouldReturnLuebenRespondWithAProvider()
        {
            var provider = _server.Given(Request.Create());
            Assert.NotNull(provider);
            Assert.IsType<LuebenRespondWithAProvider>(provider);
        }

        [Fact]
        public void StartWireMockTest()
        {
            using (var server = LuebenWireMockServer.Start(new WireMockServerSettings()))
            {
                Assert.True(server.IsStarted);
            }
        }

        [Fact]
        public void GivenGivenLuebenWireMockServer_ShouldSavePacts()
        {
            try
            {
                _server.WithConsumer("Foo")
                    .WithProvider("Bar")
                    .Given(
                        Request.Create()
                            .WithPath("/api/foo/bar"))
                    .WithTitle("Foo requests Bar")
                    .RespondWith(
                        Response.Create()
                            .WithStatusCode(200));

                _server.WithConsumer("Foo")
                    .WithProvider("App")
                    .Given(
                        Request.Create()
                            .WithPath("/api/foo/app"))
                    .WithTitle("Foo requests App")
                    .RespondWith(
                        Response.Create()
                            .WithStatusCode(200));

                _server.SavePact("./pacts");

                Assert.True(File.Exists("./pacts/pact-Foo-Bar.json"));
                var fooBarPact = JsonConvert.DeserializeObject<Pact>(File.ReadAllText("./pacts/pact-Foo-Bar.json"));
                Assert.Equal("Foo", fooBarPact.Consumer.Name);
                Assert.Equal("Bar", fooBarPact.Provider.Name);
                var interaction = Assert.Single(fooBarPact.Interactions);
                Assert.Equal("/api/foo/bar", interaction.Request.Path);
                Assert.Equal("Foo requests Bar", interaction.ProviderState);

                Assert.True(File.Exists("./pacts/pact-Foo-App.json"));
                var fooAppPact = JsonConvert.DeserializeObject<Pact>(File.ReadAllText("./pacts/pact-Foo-App.json"));
                Assert.Equal("Foo", fooAppPact.Consumer.Name);
                Assert.Equal("App", fooAppPact.Provider.Name);
                interaction = Assert.Single(fooAppPact.Interactions);
                Assert.Equal("/api/foo/app", interaction.Request.Path);
                Assert.Equal("Foo requests App", interaction.ProviderState);
            }
            finally
            {
                Directory.Delete("./pacts", true);
            }
        }

        [Fact]
        public void GivenGivenLuebenWireMockServer_WhenFileNameIsSpecified_ThenShouldSavePactsFileWithThatName()
        {
            try
            {
                _server.WithConsumer("Foo")
                    .WithProvider("Bar")
                    .Given(
                        Request.Create()
                            .WithPath("/api/foo/bar"))
                    .WithTitle("Foo requests Bar")
                    .RespondWith(
                        Response.Create()
                            .WithStatusCode(200));

                _server.SavePact("./pacts", "test-pact.json");

                Assert.True(File.Exists("./pacts/test-pact.json"));
            }
            finally
            {
                Directory.Delete("./pacts", true);
            }
        }

        public static IEnumerable<object[]> Data =>
            new List<object[]>(Enumerable.Range(1, 10).Select(x => new object[] { x }));

        [Theory]
        [MemberData(nameof(Data))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters")]
        public async Task GivenMultipleServers_ShouldBeAbleToWriteToASingleFileSuccessfully(int _)
        {
            var expectedFile = Path.Combine("./pacts", "pact-Foo-Bar.json");

            try
            {
                _server.WithConsumer("Foo")
                    .WithProvider("Bar")
                    .Given(
                        Request.Create()
                            .WithPath("/api/foo/bar"))
                    .WithTitle("Foo requests Bar")
                    .RespondWith(
                        Response.Create()
                            .WithStatusCode(200));

                using (var server1 = LuebenWireMockServer.Start(new WireMockServerSettings()))
                {
                    server1.WithConsumer("Foo")
                        .WithProvider("Bar")
                        .Given(
                            Request.Create()
                                .WithPath("/api/foo/bar/new"))
                        .WithTitle("Foo requests New Bar")
                        .RespondWith(
                            Response.Create()
                                .WithStatusCode(200));

                    var tasks = new[]
                    {
                        Task.Run(() => _server.SavePact("./pacts")),
                        Task.Run(() => server1.SavePact("./pacts")),
                    };

                    await Task.WhenAll(tasks);
                }

                Assert.Single(Directory.GetFiles("./pacts"));
                Assert.True(File.Exists(expectedFile));
                var pact = JsonConvert.DeserializeObject<Pact>(File.ReadAllText(expectedFile));

                Assert.NotNull(pact);
                Assert.Equal(2, pact.Interactions.Count);
            }
            finally
            {
                Directory.Delete("./pacts", true);
            }
        }

        public void Dispose()
        {
            _server.Stop();
            _server.Dispose();
        }
    }
}
