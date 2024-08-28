using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Lueben.Microservice.Diagnostics;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TableEntity = Azure.Data.Tables.TableEntity;

namespace Lueben.Microservice.CircuitBreaker.CleanUp
{
    public class EntityAzureStorageCleaner : IEntityAzureStorageCleaner
    {
        private const string CorrelationBlobName = "CorrelationBlobName";
        private const string InputBlobProperty = "InputBlobName";
        private readonly ILogger<EntityAzureStorageCleaner> _logger;
        private readonly IDurableClient _durableClient;
        private readonly TableClient _tableClient;

        public EntityAzureStorageCleaner(IDurableClientFactory durableClientFactory,
            IAzureClientFactory<TableServiceClient> tableServiceClientFactory,
            ILogger<EntityAzureStorageCleaner> logger,
            IOptions<DurableTaskOptions> durableOptions)
        {
            Ensure.ArgumentNotNull(tableServiceClientFactory, nameof(tableServiceClientFactory));
            Ensure.ArgumentNotNull(durableClientFactory, nameof(durableClientFactory));

            _logger = logger;
            var taskHubName = durableOptions.Value.HubName;
            _durableClient = durableClientFactory.CreateClient(new DurableClientOptions
            {
                TaskHub = taskHubName
            });

            var historyTableName = $"{taskHubName}History";
            var tableServiceClient = tableServiceClientFactory.CreateClient(Constants.CircuitBreakerHistoryTableClient);
            _tableClient = tableServiceClient.GetTableClient(historyTableName);
        }

        public virtual async Task CleanEntityHistory(EntityCleanUpOptions options)
        {
            Ensure.ArgumentNotNullOrEmpty(options.EntityName, nameof(EntityCleanUpOptions.EntityName));
            _logger.LogDebug($"{nameof(EntityCleanUpOptions.EntityName)}: {options.EntityName}");
            _logger.LogDebug($"{nameof(EntityCleanUpOptions.Ids)}: {string.Join(',', options.Ids)}");
            _logger.LogDebug($"{nameof(EntityCleanUpOptions.PurgeWithoutAnalyze)}: {options.PurgeWithoutAnalyze}");

            EntityQueryResult queryResult = null;

            try
            {
               _logger.LogDebug($"Fetching list of entities for {options.EntityName}.");
               queryResult = await _durableClient.ListEntitiesAsync(new EntityQuery { EntityName = options.EntityName, IncludeDeleted = true }, CancellationToken.None);
               _logger.LogDebug("Fetched list of entities.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get list of durable entity instances.");
            }

            if (queryResult?.Entities != null)
            {
                _logger.LogDebug($"Found {queryResult.Entities.Count()} instances.");
                foreach (var entity in queryResult.Entities)
                {
                    var instanceId = entity.EntityId.ToString();
                    _logger.LogDebug($"Analyzing {instanceId}.");

                    if (await NeedToPurgeEntity(entity, options))
                    {
                        await PurgeEntity(instanceId);
                    }
                    else
                    {
                        _logger.LogDebug($"No need to purge {instanceId}.");
                    }
                }
            }
        }

        private async Task<bool> NeedToPurgeEntity(DurableEntityStatus entity, EntityCleanUpOptions options)
        {
            var instanceId = entity.EntityId.ToString();
            var ids = options.Ids;
            var checkBlobs = !options.PurgeWithoutAnalyze;

            if (ids.Count > 0 && !ids.Contains(entity.EntityId.EntityKey))
            {
                return false;
            }

            if (!checkBlobs)
            {
                return true;
            }

            try
            {
                var cbFilter = $"PartitionKey eq '{instanceId}' and Name eq '{instanceId}'";
                _logger.LogDebug($"Getting BlobName fields for instance {instanceId}.");
                var queryResult = _tableClient.QueryAsync<TableEntity>(filter: cbFilter, maxPerPage: 1, new[] { CorrelationBlobName, InputBlobProperty });
                await foreach (var historyRecords in queryResult.AsPages())
                {
                    var tableEntity = historyRecords.Values.FirstOrDefault();
                    var blobExists = !string.IsNullOrEmpty(tableEntity?.GetString(CorrelationBlobName)) ||
                                     !string.IsNullOrEmpty(tableEntity?.GetString(InputBlobProperty));
                    if (!blobExists)
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Failed to read history for {instanceId}.");
                return false;
            }

            return true;
        }

        private async Task PurgeEntity(string instanceId)
        {
            try
            {
                var purgeResult = await _durableClient.PurgeInstanceHistoryAsync(instanceId);
                if (purgeResult?.InstancesDeleted == 1)
                {
                    _logger.LogInformation($"Successfully deleted {instanceId}.");
                }
                else
                {
                    _logger.LogWarning($"{instanceId} is not deleted.");
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Failed to delete {instanceId}.");
            }
        }
    }
}
