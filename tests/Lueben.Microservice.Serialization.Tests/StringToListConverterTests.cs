using Lueben.Microservice.Serialization.Converters;
using Moq;
using Newtonsoft.Json;

namespace Lueben.Microservice.Serialization.Tests
{
    public class StringToListConverterTests
    {
        private readonly StringToListConverter<string> _converter;
        private readonly Mock<JsonReader> _mockJsonReader;

        public StringToListConverterTests()
        {
            
            _converter = new StringToListConverter<string>();
            
            _mockJsonReader = new Mock<JsonReader>();
        }

        [Fact]
        public void GivenCommaSeparatedString_WhenConverterProcessesIt_ThenTheListOfValuesShouldBeReturned()
        {
            const string json = "string1, string2";
            _mockJsonReader.Setup(x => x.Value).Returns(json);

            var result = _converter.ReadJson(_mockJsonReader.Object, null, null, JsonSerializer.CreateDefault());

            Assert.NotNull(result);
            Assert.IsAssignableFrom<IList<string>>(result);
            var collection = (IList<string>)result;
            Assert.Equal(2, collection.Count);
            Assert.Equal("string1", collection.First());
            Assert.Equal("string2", collection.Last());
        }

        [Fact]
        public void GivenEmptyString_WhenConverterProcessesIt_ThenTheEmptyListOfValuesShouldBeReturned()
        {
            const string json = "";
            _mockJsonReader.Setup(x => x.Value).Returns(json);

            var result = _converter.ReadJson(_mockJsonReader.Object, null, null, JsonSerializer.CreateDefault());

            Assert.NotNull(result);
            Assert.IsAssignableFrom<IList<string>>(result);
            var collection = (IList<string>)result;
            Assert.Equal(0, collection.Count);
        }
    }
}