using System;
using Azure;
using Azure.Data.Tables;

namespace Lueben.Microservice.AzureTableRepository.Tests
{
    public class FirstTableRecordClass : ITableEntity
    {
        public string TestField { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}