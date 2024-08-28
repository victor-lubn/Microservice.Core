using System.Collections.Generic;
using System.Collections.Specialized;
using Lueben.Microservice.EntityFunction.Extensions;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Moq;
using Xunit;

namespace Lueben.Microservice.EntityFunction.Tests
{
    public class QueryStringCollectionExtensionsTests
    {
        [Fact]
        public void GivenToObject_WhenExecuted_ThenShouldConvertDictionaryToObject()
        {
            var obj = new TestModel
            {
                Id = 1,
                Test = "test"
            };

            var dict = new Dictionary<string, string>
            {
                { nameof(TestModel.Id), obj.Id.ToString() },
                { nameof(TestModel.Test), obj.Test }
            };

            var result = dict.ToObject<TestModel>();

            Assert.NotNull(result);
            Assert.Equal(obj.Id, result.Id);
            Assert.Equal(obj.Test, result.Test);
        }

        [Fact]
        public void GivenToDictionary_WhenExecutedAndNullSourcePassed_ThenEmptyDictionaryIsReturned()
        {
            NameValueCollection sourceCollection = null;

            var result = sourceCollection.ToDictionary();

            Assert.Empty(result);
        }

        [Fact]
        public void GivenToDictionary_WhenExecutedWithNotNullCollection_ThenConvertCollectionToDictionary()
        {
            var key = "id";
            var sourceCollection = new NameValueCollection
            {
                { key, "1" }
            };

            var result = sourceCollection.ToDictionary();

            Assert.NotEmpty(result);
            Assert.Equal(1, result.Count);
            Assert.True(result.ContainsKey(key));
        }

        [Fact]
        public void GivenToQueryObject_WhenExecuted_ThenConvertQueryParamsToExpectedObject()
        {
            var key = nameof(TestModel.Id);
            var sourceCollection = new NameValueCollection
            {
                { key, "1" }
            };

            var context = new Mock<FunctionContext>();
            var request = new Mock<HttpRequestData>(context.Object);
            request.Setup(r => r.Query).Returns(sourceCollection);

            var result = request.Object.ToQueryObject<TestModel>();

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }
    }
}
