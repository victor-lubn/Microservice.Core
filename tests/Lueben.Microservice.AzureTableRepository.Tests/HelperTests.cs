using Azure.Core;
using System;
using Xunit;

namespace Lueben.Microservice.AzureTableRepository.Tests
{
    public class HelperTests
    {
        [Fact]
        public void GivenGetTableName_WhenTableOptionsAreNull_ThenTypeNameReturned()
        {
            AzureTableRepositoryOptions tableOptions = null;

            var result = Helpers.GetTableName<TestEntity>(tableOptions);

            Assert.Equal(nameof(TestEntity), result);
        }

        [Fact]
        public void GivenGetTableName_WhenTableNameIsNull_ThenTypeNameReturned()
        {
            var tableOptions = new AzureTableRepositoryOptions();

            var result = Helpers.GetTableName<TestEntity>(tableOptions);

            Assert.Equal(nameof(TestEntity), result);
        }

        [Fact]
        public void GivenGetTableName_WhenTableNameDoesNotMatch_ThenInvalidOperationExceptionIsThrown()
        {
            var tableOptions = new AzureTableRepositoryOptions
            {
                TableName = "!_not_matched"
            };

            Assert.Throws<InvalidOperationException>(() => Helpers.GetTableName<TestEntity>(tableOptions));
        }

        [Fact]
        public void GivenGetTableName_WhenTableNameMatches_ThenTableNameReturned()
        {
            var tableOptions = new AzureTableRepositoryOptions
            {
                TableName = "matched"
            };

            var result = Helpers.GetTableName<TestEntity>(tableOptions);

            Assert.Equal(tableOptions.TableName, result);
        }

        [Fact]
        public void GivenGetTableServiceClientName_WhenTableOptionsAreNull_ThenDefaultNameReturned()
        {
            AzureTableRepositoryOptions tableOptions = null;

            var result = Helpers.GetTableServiceClientName(tableOptions);

            Assert.Equal(Constants.DefaultTableServiceClientName, result);
        }

        [Fact]
        public void GivenGetTableServiceClientName_WhenConnectionIsNull_ThenDefaultNameReturned()
        {
            var tableOptions = new AzureTableRepositoryOptions();

            var result = Helpers.GetTableServiceClientName(tableOptions);

            Assert.Equal(Constants.DefaultTableServiceClientName, result);
        }

        [Fact]
        public void GivenGetTableServiceClientName_WhenConnectionIsNotNull_ThenConnectionIsReturned()
        {
            var tableOptions = new AzureTableRepositoryOptions
            {
                Connection = "matched"
            };

            var result = Helpers.GetTableServiceClientName(tableOptions);

            Assert.Equal(tableOptions.Connection, result);
        }
    }

    internal class TestEntity
    {
    }
}
