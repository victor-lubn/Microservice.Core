using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Lueben.Integration.Testing.WireMock.Utils;
using WireMock.Admin.Mappings;
using WireMock.Pact.Models.V2;
using Xunit;

namespace Lueben.Integration.Testing.WireMock.Tests.Utils
{
    public class LuebenPactMapperTests
    {
        private readonly IFixture _fixture;

        public LuebenPactMapperTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public void GivenLuebenPactMapper_WhenSerializingPact_ThenShouldReturnByteArray()
        {
            var pact = _fixture.Create<Pact>();
            var bytes = LuebenPactMapper.SerializeAsPactFile(pact);
            Assert.NotNull(bytes);
        }

        [Fact]
        public void GivenLuebenPactMapper_WhenConvertingMappingsToPact_AndConsumerIsNull_ThenPactConsumerShouldHaveDefaultValue()
        {
            var providerMapping = KeyValuePair.Create("Foo", _fixture.CreateMany<MappingModel>().ToList());
            var (filename, pact) = LuebenPactMapper.ToPactObject(null, providerMapping);

            Assert.NotNull(filename);
            Assert.NotNull(pact);
            Assert.Equal("Default Consumer", pact.Consumer.Name);
        }

        [Fact]
        public void GivenLuebenPactMapper_WhenConvertingMappingsToPact_AndProviderIsNull_ThenPactProviderShouldHaveDefaultValue()
        {
            var providerMapping = KeyValuePair.Create((string)null, _fixture.CreateMany<MappingModel>().ToList());
            var (filename, pact) = LuebenPactMapper.ToPactObject("Foo", providerMapping);

            Assert.NotNull(filename);
            Assert.NotNull(pact);
            Assert.Equal("Default Provider", pact.Provider.Name);
        }

        [Fact]
        public void GivenLuebenPactMapper_WhenConvertingMappingsToPact_AndFilenameIsPassed_ThenFilenameShouldBeOverriden()
        {
            var providerMapping = KeyValuePair.Create("Bar", _fixture.CreateMany<MappingModel>().ToList());
            var (filename, pact) = LuebenPactMapper.ToPactObject("Foo", providerMapping, "file.json");

            Assert.Equal("file.json", filename);
        }

        [Fact]
        public void GivenLuebenPactMapper()
        {
            var mappings = _fixture.Build<MappingModel>()
                .With(x => x.Request, _fixture.Build<RequestModel>()
                    .With(r => r.Path, "/api/foo/bar")
                    .Create())
                .With(x => x.Response, _fixture.Build<ResponseModel>()
                    .With(x => x.StatusCode, 200)
                    .Create())
                .CreateMany(2)
                .ToList();
            var providerMapping = KeyValuePair.Create("bar", mappings);
            var (filename, pact) = LuebenPactMapper.ToPactObject("foo", providerMapping);

            Assert.Equal("pact-foo-bar.json", filename);
            Assert.NotNull(pact);
            Assert.Equal("bar", pact.Provider.Name);
            Assert.Equal("foo", pact.Consumer.Name);
            Assert.Equal(2, pact.Interactions.Count);
        }
    }
}
