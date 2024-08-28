using Lueben.Microservice.Serialization.Converters;
using Newtonsoft.Json;

namespace Lueben.Microservice.Serialization.Tests
{
    public class ShortDateConverterTests
    {
        [Fact]
        public void GivenShortDateTimeConverter_WhenConvertingDateTime_ThenShouldConvertAccordingToFormat()
        {
            var converter = new ShortDateConverter();
            Assert.Equal("yyyy-MM-dd", converter.DateTimeFormat);

            var dt = DateTime.Now;
            var value = JsonConvert.SerializeObject(dt, converter);
            Assert.Equal(dt.ToString("yyyy-MM-dd"), value.Trim('"'));
        }
    }
}