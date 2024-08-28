using Lueben.Microservice.ApplicationInsights.Helpers;
using Xunit;

namespace Lueben.Microservice.ApplicationInsights.Tests
{
    public class FunctionPropertyHelperTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(FunctionPropertyHelper.FunctionCustomPropertyPrefix + "test", "test")]
        [InlineData("test", "test")]
        public void GivenGetOriginalPropertyName_WhenCalled_ThenExpectedValueIsReturned(string input, string expectedValue)
        {
            var actualValue = FunctionPropertyHelper.GetOriginalPropertyName(input);

            Assert.Equal(expectedValue, actualValue);
        }
    }
}
