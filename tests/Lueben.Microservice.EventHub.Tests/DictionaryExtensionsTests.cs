using Lueben.Microservice.EventHub.Extensions;
using System.Collections.Generic;
using Xunit;

namespace Lueben.Microservice.EventHub.Tests
{
    public class DictionaryExtensionsTests
    {
        [Fact]
        public void GivenAddIfNotNull_WhenValueIsNull_ThenShouldNotAddToTheDictionary()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.AddIfNotNull("key", null);

            Assert.Empty(dictionary);
        }

        [Fact]
        public void GivenAddIfNotNull_WhenValueIsNotNull_ThenShouldAddToTheDictionary()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.AddIfNotNull("key", "value");

            Assert.True(dictionary.ContainsKey("key"));
        }
    }
}
