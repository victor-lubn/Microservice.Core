namespace Lueben.Microservice.Api.ValidationFunctionTests.Models
{
    public class TestEmptyClass
    {
        public IList<string> TestList { get; set; } = new List<string>();

        public string[]? TestArray { get; set; }

        public int? TestIntProperty { get; set; }

        public string? TestStringProperty { get; set; }
    }
}
