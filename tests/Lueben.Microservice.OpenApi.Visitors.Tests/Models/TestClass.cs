using System.ComponentModel;
using Lueben.Microservice.Serialization.Converters;
using Newtonsoft.Json;

namespace Lueben.Microservice.OpenApi.Visitors.Tests.Models
{
    public class TestClass
    {
        public DateTime? Created { get; set; }

        public string StringProperty { get; set; } = string.Empty;

        public TestEnum? EnumProperty { get; set; }

        [ReadOnly(true)]
        public string ReadOnlyProperty { get; } = "test";

        [JsonConverter(typeof(ShortDateConverter))]
        public DateTime? ShortDateProperty { get; set; }
    }

    public enum TestEnum
    {
        One,
        Two
    }
}
