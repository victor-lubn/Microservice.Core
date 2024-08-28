using System.Linq;
using Lueben.Microservice.Api.Idempotency.Extensions;
using Xunit;

namespace Lueben.Microservice.Api.Idempotency.Tests
{
    public class ListExtensionsTests
    {
        [Theory]
        [InlineData(1, 1)]
        [InlineData(10, 1)]
        [InlineData(10, 2)]
        public void GivenSplitList_WhenCalled_ThenReturnsListOfListsWithSpecifiedSize(int originalListSize, int splitListSize)
        {
            var originalList = Enumerable.Range(1, originalListSize).ToList();
            var expectedResult = originalListSize / splitListSize;

            var actualResult = originalList.SplitList(splitListSize);

            Assert.Equal(expectedResult, actualResult.Count());
        }
    }
}
