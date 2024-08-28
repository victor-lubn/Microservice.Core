namespace Lueben.ApplicationInsights.Tests
{
    public class PropertyHelperTests
    {
        [Theory]
        [InlineData("test", Constants.CompanyPrefix+"_test")]
        [InlineData(Constants.CompanyPrefix+ "_test", Constants.CompanyPrefix+ "_test")]
        public void GivenGetApplicationPropertyName_WhenPropertyDoesNotContainCompanyName_ThenAddCompanyNameToProperty(string property, string expectedResult)
        {
            var actualResult = PropertyHelper.GetApplicationPropertyName(property);

            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("test", "HWN_Data_test")]
        [InlineData("HWN_test", "HWN_test")]
        public void GivenGetCustomDataPropertyName_WhenPropertyDoesNotContainCompanyName_ThenAddCompanyNameAndDataPrefixToProperty(string property, string expectedResult)
        {
            var actualResult = PropertyHelper.GetCustomDataPropertyName(property);

            Assert.Equal(expectedResult, actualResult);
        }
    }
}
