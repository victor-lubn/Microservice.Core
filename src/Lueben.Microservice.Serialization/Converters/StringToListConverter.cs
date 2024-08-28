using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Lueben.Microservice.Serialization.Converters
{
    public class StringToListConverter<T> : JsonConverter
    {
        private const string Separator = ",";

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(string.Join(Separator, (List<T>)value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var values = ((string)reader.Value)?.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

            if (values == null)
            {
                return new List<T>();
            }

            var trimmedValues = values.Select(x => x.Trim()).ToList();

            var json = JsonConvert.SerializeObject(trimmedValues);

            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<T>);
        }
    }
}