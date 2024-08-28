using System;
using System.Collections.Generic;
using Azure.Data.Tables;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lueben.Microservice.AzureTableRepository.Tests
{
    public class AddTableServiceClientTests
    {
        private readonly ServiceCollection _services;

        public AddTableServiceClientTests()
        {
            _services = new ServiceCollection();
        }

        [Fact]
        public void GivenAddTableServiceClient_WhenNoTableAttribute_ThenExceptionIsRaised()
        {
        }

        [Fact]
        public void GivenAddAzureTableRepository_WhenNoConnectionNamePassed_ThenDefaultIsUsed()
        {
            const string accountName = "devstoreaccount";
            const string connectionString = "DefaultEndpointsProtocol=http;AccountName=" + accountName + ";AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount;";
            var json = $"{{\"AzureWebJobsStorage\":\"{connectionString}\"}}";
            _services.AddConfiguration(json);
            _services.AddAzureTableRepository<FirstTableRecordClass>(new AzureTableRepositoryOptions());

            var provider = _services.BuildServiceProvider();

            var defaultClient = provider.GetService<IAzureClientFactory<TableServiceClient>>().CreateClient(Constants.DefaultTableServiceClientName);

            Assert.Equal(accountName, defaultClient.AccountName);
        }

        [Fact]
        public void GivenAddAzureTableRepository_WhenConnectionIsDefinedButWrongTableName_ThenExceptionIsRaised()
        {
            const string connection = nameof(AzureTableRepositoryOptions.Connection);
            const string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount;";
            var json = $"{{\"ConnectionStrings:{connection}\":\"{connectionString}\"}}";
            _services.AddConfiguration(json);

            Assert.Throws<InvalidOperationException>(() => _services.AddAzureTableRepository<FirstTableRecordClass>(new AzureTableRepositoryOptions
            {
                TableName = "!@@@@"
            }));
        }

        [Fact]
        public void GivenAddAzureTableRepository_WhenConnectionIsDefined_ThenClientIsRegisteredWithCorrectName()
        {
            const string connection = nameof(AzureTableRepositoryOptions.Connection);
            const string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount;";
            var json = $"{{\"ConnectionStrings:{connection}\":\"{connectionString}\",\r\n";
            json += $"\"AzureWebJobsStorage\":\"{connectionString}\"}}";
            _services.AddConfiguration(json);

            _services.AddAzureTableRepository<FirstTableRecordClass>(new AzureTableRepositoryOptions { Connection = connection });
            _services.AddAzureTableRepository<SecondTableRecordClass>(new AzureTableRepositoryOptions());

            var provider = _services.BuildServiceProvider();
            var factory1 = provider.GetService<AzureTableRepository<FirstTableRecordClass>>();
            var factory2 = provider.GetService<AzureTableRepository<SecondTableRecordClass>>();
            Assert.NotNull(factory1);
            Assert.NotNull(factory2);
        }

        [Fact]
        public void GivenAddAzureTableRepositoryFromOptions_WhenNoTableSpecificSettings_ThenDefaultClientIsRegisteredWithCorrectName()
        {
            const string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount;";
            var json = $"{{\"AzureWebJobsStorage\":\"{connectionString}\"}}";
            _services.AddConfiguration(json);

            _services.AddAzureTableRepositoriesFromOptions(new List<Type> { typeof(FirstTableRecordClass), typeof(SecondTableRecordClass) });

            var provider = _services.BuildServiceProvider();
            provider.GetService<AzureTableRepository<FirstTableRecordClass>>();
            provider.GetService<AzureTableRepository<SecondTableRecordClass>>();
        }

        [Fact]
        public void GivenAddAzureTableRepositoryFromOptions_WhenClientForNotDefaultConnectionIsNotRegistered_ThenRepositoryIsNotCreated()
        {
            const string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount;";
            var json = $"{{\"AzureWebJobsStorage\":\"{connectionString}\",";
            json += $"\"{nameof(AzureTableRepositoryOptions)}:{nameof(SecondTableRecordClass)}:{nameof(AzureTableRepositoryOptions.Connection)}\":\"TestConnection\"}}";
            _services.AddConfiguration(json);

            _services.AddAzureTableRepositoriesFromOptions(new List<Type> { typeof(FirstTableRecordClass) });

            var provider = _services.BuildServiceProvider();
            var repository = provider.GetService<AzureTableRepository<FirstTableRecordClass>>();
            Assert.NotNull(repository);

            Assert.Throws<InvalidOperationException>(() => provider.GetService<AzureTableRepository<SecondTableRecordClass>>());
        }

        [Fact]
        public void GivenAddAzureTableRepositoryFromOptions_WhenNoOptionsConfigured_ThenDefaulrsettingsAreUsed()
        {
            const string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount;";
            var json = $"{{\"AzureWebJobsStorage\":\"{connectionString}\"}}";
            _services.AddConfiguration(json);

            _services.AddAzureTableRepositoriesFromOptions(new List<Type> { typeof(FirstTableRecordClass) });

            var provider = _services.BuildServiceProvider();
            var repository = provider.GetService<AzureTableRepository<FirstTableRecordClass>>();
            Assert.NotNull(repository);
            var repository2 = provider.GetService<AzureTableRepository<SecondTableRecordClass>>();
            Assert.NotNull(repository2);
        }
    }
}