using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Lueben.Microservice.Api.Idempotency.Constants;
using Lueben.Microservice.Api.Idempotency.Exceptions;
using Lueben.Microservice.Api.Idempotency.Extensions;
using Lueben.Microservice.Api.Idempotency.Models;
using Lueben.Microservice.Api.Idempotency.Options;
using Lueben.Microservice.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.Api.Idempotency.IdempotencyDataProviders
{
    public class AzureTableIdempotencyDataProvider : IIdempotencyDataProvider<IdempotencyEntity>
    {
        private const int MaxTransactionCount = 100;
        private const int MaxPageSize = 1000;

        private readonly ILogger<AzureTableIdempotencyDataProvider> _logger;
        private readonly TableClient _tableClient;

        public AzureTableIdempotencyDataProvider(
            ILogger<AzureTableIdempotencyDataProvider> logger,
            IConfiguration configuration,
            IOptionsSnapshot<IdempotencyAzureTableOptions> options)
        {
            Ensure.ArgumentNotNull(logger, nameof(logger));
            Ensure.ArgumentNotNull(configuration, nameof(configuration));
            Ensure.ArgumentNotNull(options, nameof(options));

            _logger = logger;
            var connectionString = configuration.GetValue<string>("AzureWebJobsStorage");

            _tableClient = new TableClient(connectionString, options.Value.TableName);
            _tableClient.CreateIfNotExists();
        }

        public AzureTableIdempotencyDataProvider(ILogger<AzureTableIdempotencyDataProvider> logger, TableClient tableClient)
        {
            Ensure.ArgumentNotNull(logger, nameof(logger));
            Ensure.ArgumentNotNull(tableClient, nameof(tableClient));

            _logger = logger;
            _tableClient = tableClient;
            _tableClient.CreateIfNotExists();
        }

        public async Task Add(string idempotencyKey, string payloadHash, string functionName)
        {
            try
            {
                await _tableClient.AddEntityAsync(new IdempotencyEntity
                {
                    PartitionKey = idempotencyKey,
                    RowKey = idempotencyKey,
                    PayloadHash = payloadHash,
                    FunctionName = functionName,
                });
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                throw new IdempotencyConflictException();
            }
        }

        public async Task CleanUp()
        {
            var epoch = DateTime.UnixEpoch.ToString(Formats.AzureTableDateTimeFormat);
            var limit = DateTime.UtcNow.AddDays(-1).ToString(Formats.AzureTableDateTimeFormat);

            var query = $"Timestamp ge datetime'{epoch}' and Timestamp lt datetime'{limit}'";
            var queryResultFilter = _tableClient.Query<IdempotencyEntity>(
                query, MaxPageSize, select: new List<string> { nameof(ITableEntity.PartitionKey), nameof(ITableEntity.RowKey), nameof(ITableEntity.ETag) });

            var deleted = 0;
            foreach (var page in queryResultFilter.AsPages())
            {
                var actions = page.Values
                    .Select(e => _tableClient.DeleteEntityAsync(e.PartitionKey, e.RowKey, e.ETag)).ToList();
                deleted += actions.Count;

                var actionChunks = actions.SplitList(MaxTransactionCount);
                foreach (var action in actionChunks)
                {
                    await Task.WhenAll(action);
                }
            }

            _logger.LogInformation($"{deleted} idempotency keys were deleted to {limit}");
        }

        public async Task Delete(IdempotencyEntity entity)
        {
            await _tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, entity.ETag);
        }

        public async Task<IdempotencyEntity> Get(string idempotencyKey)
        {
            IdempotencyEntity entity;
            try
            {
                entity = await _tableClient.GetEntityAsync<IdempotencyEntity>(idempotencyKey, idempotencyKey);
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                return null;
            }

            return entity;
        }

        public async Task Update(IdempotencyEntity idempotency)
        {
            await _tableClient.UpdateEntityAsync(idempotency, idempotency.ETag);
        }
    }
}
