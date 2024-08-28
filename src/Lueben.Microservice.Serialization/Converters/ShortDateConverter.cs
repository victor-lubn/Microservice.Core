using Newtonsoft.Json.Converters;

namespace Lueben.Microservice.Serialization.Converters
{
    public class ShortDateConverter : IsoDateTimeConverter
    {
        public const string Format = "yyyy-MM-dd";

        public ShortDateConverter()
        {
            DateTimeFormat = Format;
        }
    }
}