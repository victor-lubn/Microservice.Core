using Lueben.Microservice.Api.ValidationFunction.Extensions;
using Lueben.Microservice.Api.ValidationFunctionTests.Models;

namespace Lueben.Microservice.Api.ValidationFunction.Tests
{
    public class TypeExtensionsTests
    {
        [Fact]
        public void GivenGetFilledProperties_WhenCalledANdNoValueIsPassed_ThenArgumentNullExceptionIsThrown()
        {
            TestClass? obj = null;

            Assert.Throws<ArgumentNullException>(() => obj.GetFilledProperties());
        }

        [Fact]
        public void GivenGetFilledProperties_WhenCalledOnObjectWithEmptyProperties_ThenEmptyListIsReturned()
        {
            var obj = new TestEmptyClass();

            var result = obj.GetFilledProperties();

            Assert.Empty(result);
        }

        [Fact]
        public void GivenGetFilledProperties_WhenCalledOnObjectWithPopulatedProperties_ThenListOfFilledPropertiesIsReturned()
        {
            var obj = new TestClass();

            var result = obj.GetFilledProperties();

            Assert.NotEmpty(result);
            Assert.Contains(nameof(obj.Id), result);
        }
    }
}
