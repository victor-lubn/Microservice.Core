using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Lueben.Microservice.Diagnostics;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.AzureTableRepository
{
    public class AzureTableRepository<T>
        where T : class, ITableEntity, new()
    {
        private Lazy<TableClient> _initializedTableClient;

        protected TableClient TableClient => _initializedTableClient.Value;

        public AzureTableRepository(IAzureClientFactory<TableServiceClient> tableServiceClientFactory, IOptionsSnapshot<AzureTableRepositoryOptions> tableOptions)
        {
            Ensure.ArgumentNotNull(tableServiceClientFactory, nameof(tableServiceClientFactory));

            var typeName = typeof(T).Name;
            var options = tableOptions?.Get(typeName);
            var clientName = Helpers.GetTableServiceClientName(options);

            var tableServiceClient = tableServiceClientFactory.CreateClient(clientName);
            var tableClient = tableServiceClient.GetTableClient(Helpers.GetTableName<T>(options));

            CreateInitializedTableClient(tableClient);
        }

        public AzureTableRepository(TableServiceClient tableServiceClient, string tableName) : this(tableServiceClient.GetTableClient(tableName))
        {
            Ensure.ArgumentNotNull(tableServiceClient, nameof(tableServiceClient));
        }

        public AzureTableRepository(TableClient tableClient)
        {
            Ensure.ArgumentNotNull(tableClient, nameof(TableClient));
            CreateInitializedTableClient(tableClient);
        }

        public virtual async Task AddAsync(T entity)
        {
            await TableClient.AddEntityAsync(entity).ConfigureAwait(false);
        }

        public virtual async Task<T> GetAsync(string partitionKey, string rowKey)
        {
            var response = await TableClient.GetEntityAsync<T>(partitionKey, rowKey).ConfigureAwait(false);
            return response.Value;
        }

        public virtual async Task UpsertAsync(T entity)
        {
            await TableClient.UpsertEntityAsync(entity).ConfigureAwait(false);
        }

        public virtual async Task RemoveAsync(string partitionKey, string rowKey)
        {
            await TableClient.DeleteEntityAsync(partitionKey, rowKey).ConfigureAwait(false);
        }

        public virtual IEnumerable<T> Query(string query = null, int? pageSize = null, CancellationToken cancellationToken = default)
        {
            return TableClient.Query<T>(query, pageSize, null, cancellationToken);
        }

        public virtual IAsyncEnumerable<T> QueryAsync(string query = null, int? pageSize = null, CancellationToken cancellationToken = default)
        {
            return TableClient.QueryAsync<T>(query, pageSize, null, cancellationToken);
        }

        public virtual IEnumerable<T> Query(Expression<Func<T, bool>> expression = null, int? pageSize = null, CancellationToken cancellationToken = default)
        {
            return TableClient.Query(expression, pageSize, null, cancellationToken);
        }

        public virtual IAsyncEnumerable<T> QueryAsync(Expression<Func<T, bool>> expression = null, int? pageSize = null, CancellationToken cancellationToken = default)
        {
            return TableClient.QueryAsync(expression, pageSize, null, cancellationToken);
        }

        public virtual async Task BatchRemoveAsync(IEnumerable<T> entities)
        {
            var entityPartitions = entities.GroupBy(e => e.PartitionKey);
            foreach (var entityPartition in entityPartitions)
            {
                var actions = entityPartition.Select(entity => new TableTransactionAction(TableTransactionActionType.Delete, entity));
                var chunks = SplitList(actions.ToList(), Constants.MaxActionsInTransaction);
                foreach (var chunk in chunks)
                {
                    await TableClient.SubmitTransactionAsync(chunk).ConfigureAwait(false);
                }
            }
        }

        protected static IEnumerable<List<TableTransactionAction>> SplitList(List<TableTransactionAction> list, int size)
        {
            for (var i = 0; i < list.Count; i += size)
            {
                yield return list.GetRange(i, Math.Min(size, list.Count - i));
            }
        }

        private void CreateInitializedTableClient(TableClient tableClient)
        {
            _initializedTableClient = new Lazy<TableClient>(() =>
            {
                tableClient.CreateIfNotExists();
                return tableClient;
            });
        }
    }
}