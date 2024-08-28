using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using Lueben.Microservice.Json.Encrypt.Configurations;
using Lueben.Microservice.Json.Encrypt.Exceptions;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Lueben.Microservice.Json.Encrypt.Tests
{
    public class EncryptedStringPropertyResolverTests
    {
        private readonly IFixture _fixture;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly Mock<IOptions<EncryptionOptions>> _optionsSnapshotMock;

        public EncryptedStringPropertyResolverTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var secretKey = "99999999999999999999999999999999";
            _optionsSnapshotMock = _fixture.Freeze<Mock<IOptions<EncryptionOptions>>>();
            _optionsSnapshotMock.Setup(x => x.Value)
                .Returns(new EncryptionOptions { Secret = secretKey });

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new EncryptedStringPropertyResolver(_optionsSnapshotMock.Object),
            };
        }

        [Fact]
        public void GivenSerializeObjectWithEncryptedStringPropertyResolver_WhenNoJsonEncryptAttribute_ThenSerializeWithoutEncryptedFields()
        {
            var objectToSerialize = new
            {
                TestString = "TestString",
                TestInt = 10,
            };

            var result = JsonConvert.SerializeObject(objectToSerialize, _jsonSerializerSettings);

            var json = JObject.Parse(result);
            Assert.Equal(objectToSerialize.TestString, json["TestString"]);
            Assert.Equal(objectToSerialize.TestInt, json["TestInt"]);
        }

        [Fact]
        public void GivenSerializeObjectWithEncryptedStringPropertyResolver_WhenJsonEncryptAttributeExists_ThenSerializeWithEncryptedFields()
        {
            var testData = _fixture.Build<TestEncryption>()
                .With(m => m.TestEmptyString, (string)null)
                .With(m => m.TestEmptyLong, (long?)null)
                .Create();
            var result = JsonConvert.SerializeObject(testData, _jsonSerializerSettings);

            var json = JObject.Parse(result);
            Assert.NotEqual(testData.TestString, json.Value<string>("TestString"));
            Assert.NotEqual(JsonConvert.SerializeObject(testData.TestLong), json.Value<string>("TestLong"));
            Assert.NotEqual(JsonConvert.SerializeObject(testData.TestDateTime), json.Value<string>("TestDateTime"));
            Assert.NotEqual(testData.TestGuid.ToString(), json.Value<string>("TestGuid"));
            Assert.False(json["TestStrings"].HasValues);
            Assert.True(json["TestEmptyString"] == null || json["TestEmptyString"].Type == JTokenType.Null);
            Assert.True(json["TestEmptyLong"] == null || json["TestEmptyLong"].Type == JTokenType.Null);
        }

        [Fact]
        public void GivenSerializeObjectWithEncryptedStringPropertyResolver_WhenJsonEncryptAttributeExists_ThenCorrectDeserializeFields()
        {
            var testData = _fixture.Build<TestEncryption>()
                .With(m => m.TestEmptyString, (string)null)
                .With(m => m.TestEmptyLong, (long?)null)
                .Create();
            var serializedString = JsonConvert.SerializeObject(testData, _jsonSerializerSettings);
            var result = JsonConvert.DeserializeObject<TestEncryption>(serializedString, _jsonSerializerSettings);

            Assert.Equal(testData.TestString, result.TestString);
            Assert.Equal(testData.TestEmptyString, result.TestEmptyString);
            Assert.Equal(testData.TestLong, result.TestLong);
            Assert.Equal(testData.TestEmptyLong, result.TestEmptyLong);
            Assert.Equal(testData.TestDateTime, result.TestDateTime);
            Assert.Equal(testData.TestGuid, result.TestGuid);
            Assert.True(testData.TestStrings.All(s => result.TestStrings.Contains(s)));
        }

        [Fact]
        public void GivenSerializeObjectWithEncryptedStringPropertyResolver_WhenSecretKeyIsNotValid_ThenThEncryptionTokenInvalidExceptionShouldBeThrown()
        {
            _optionsSnapshotMock.Setup(x => x.Value)
                .Returns(new EncryptionOptions { Secret = "WrongKey" });

            var testData = _fixture.Build<TestEncryption>()
                .With(m => m.TestEmptyString, (string)null)
                .With(m => m.TestEmptyLong, (long?)null)
                .Create();

            Assert.Throws<EncryptionTokenInvalidException>(() => JsonConvert.SerializeObject(testData, _jsonSerializerSettings));
        }

        private class TestEncryption
        {
            [JsonEncrypt]
            public string TestString { get; set; }

            [JsonEncrypt]
            public string TestEmptyString { get; set; }

            [JsonEncrypt]
            public long TestLong { get; set; }

            [JsonEncrypt]
            public long? TestEmptyLong { get; set; }

            [JsonEncrypt]
            public DateTime TestDateTime { get; set; }

            [JsonEncrypt]
            public Guid TestGuid { get; set; }

            [JsonEncrypt]
            public IEnumerable<string> TestStrings { get; set; }
        }
    }
}
