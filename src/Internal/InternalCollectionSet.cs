using Cosmos.Entity.Mapper.Extensions;
using Cosmos.Entity.Mapper.Schema;
using Cosmos.Entity.Mapper.Utilities;
using Cosmos.Entity.Mapper.Validations;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.Entity.Mapper.Internal
{
    /// <summary>
    /// This is an internal API to support the creation of the <see cref="CollectionSet{TEntity}"/> and is not generally available
    /// It may be changed or removed without notice. Use with caution as it could affect the behavior of your application
    /// </summary>
    public class InternalCollectionSet<TDocument> : CollectionSet<TDocument>, IQueryable<TDocument>
        where TDocument : class
    {
        private IQueryable<TDocument> _collectionQueryable;

        private readonly ContextOptionsBase _configurationOptions;

        /// <summary>
        /// Instantiate base container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="configuratationOptions"></param>
        public InternalCollectionSet(Container container, ContextOptionsBase configuratationOptions) : base(container)
        {
            SchemaValidationBase<TDocument>.MustHaveMinumumSchemaDefinitionOrThrow();
            CreateEntityQueryable();
            _configurationOptions = configuratationOptions;
        }

        /// <inheritdoc />
        private void CreateEntityQueryable(string partition = null)
        {
            QueryRequestOptions queryRequestOption = new QueryRequestOptions { MaxConcurrency = -1, EnableScanInQuery = true, MaxBufferedItemCount = -1, PopulateIndexMetrics = true };
            if (!string.IsNullOrEmpty(partition))
            {
                queryRequestOption.PartitionKey = new PartitionKey(partition);
            }
            _collectionQueryable = Container.GetItemLinqQueryable<TDocument>(true, null, queryRequestOption);
        }

        /// <inheritdoc />
        public override async Task<TDocument> FindAsync(string id, string partitionKey = null, CancellationToken cancellationToken = default)
        {
            EntityValidationBase.NotNullOrThrow(id);
            var partition = !string.IsNullOrEmpty(partitionKey) ? new PartitionKey(partitionKey) : PartitionKey.None;
            ItemResponse<TDocument> response = await OperationsExecutor
                .ExecuteAsync(
                    Container.ReadItemAsync<TDocument>(id, partition, new ItemRequestOptions { EnableContentResponseOnWrite = true }, cancellationToken)
                );
            return response?.Resource;
        }

        /// <inheritdoc />
        public override CollectionSet<T> OfType<T>()
        {
            return new InternalCollectionSet<T>(Container,_configurationOptions);
        }

        /// <inheritdoc />
        IQueryProvider IQueryable.Provider => _collectionQueryable.Provider;

        /// <inheritdoc />
        Expression IQueryable.Expression => _collectionQueryable.Expression;

        /// <inheritdoc />
        Type IQueryable.ElementType => _collectionQueryable.ElementType;

        /// <inheritdoc />
        IEnumerator<TDocument> IEnumerable<TDocument>.GetEnumerator() => _collectionQueryable.GetEnumerator();

        /// <inheritdoc />
        public override IEnumerator<TDocument> GetEnumerator() => _collectionQueryable.GetEnumerator();

        /// <inheritdoc />
        public override async Task<TDocument> AddAsync(TDocument entity, string partitionKey = null, CancellationToken cancellationToken = default)
        {
            EntityValidationBase.NotNullOrThrow(entity);
            var partition = Schema.ReadAttributeValue<DocumentPartitionAttribute>(entity) ?? partitionKey;
            var resolvedPartitionKey = partition is null ? PartitionKey.None : new PartitionKey(partition.ToString());
            return await OperationsExecutor.ExecuteAsync(Container.CreateItemAsync(entity, resolvedPartitionKey, new ItemRequestOptions { EnableContentResponseOnWrite = true }, cancellationToken));
        }

        /// <inheritdoc />
        public override async Task<int> AddRangeAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default)
        {
            EntityValidationBase.NotEmptyOrThrow(documents);
            var documentsGroupedByPartition = new Dictionary<string, IEnumerable<TDocument>>();

            // The cosmos SDK performs better in bulk mode when items to be created share the same partition.
            // Enabling that requires grouping the documents by the partition key and create a batch of execution for each group
            foreach(var document in documents)
            {
                var partitionKey = Schema.ReadAttributeValue<DocumentPartitionAttribute>(document);
                EntityValidationBase.NotNullOrThrow(partitionKey);
                if (documentsGroupedByPartition.ContainsKey(partitionKey.ToString()))
                {
                    documentsGroupedByPartition[partitionKey.ToString()] = documentsGroupedByPartition[partitionKey.ToString()].Append(document);
                }
                else
                {
                    documentsGroupedByPartition.Add(partitionKey.ToString(), new List<TDocument> { document });
                }
            }

            List<Func<Task>> tasksPerPartition = new();
            int numberOfSuccessfulOperations = 0;

            // Go through each group and create delegates to execute
            foreach(var documentGroup in documentsGroupedByPartition)
            {
                var documentsInPartition = documentGroup.Value;
                Task func() => Task.Run(async () =>
                {
                    List<Func<Task>> partitionTasks = new(documentsInPartition.Count());
                    foreach(var document in documentsInPartition)
                    {
                        Task documentTask() => Task.Run(async () =>
                        {                            
                            await OperationsExecutor.ExecuteAsync(Container.CreateItemAsync(document, new PartitionKey(documentGroup.Key), new ItemRequestOptions { EnableContentResponseOnWrite = false }, cancellationToken));
                            Interlocked.Increment(ref numberOfSuccessfulOperations);
                        });
                        partitionTasks.Add(documentTask);
                    }
                    await partitionTasks.ExecuteInParallelAsync(_configurationOptions.BulkBatchSize);
                });
                tasksPerPartition.Add(func);
            }
            await tasksPerPartition.ExecuteInParallelAsync();
            return numberOfSuccessfulOperations;
        }

        /// <inheritdoc />
        /// <param name="document"></param>
        /// <param name="partitionKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<TDocument> UpdateAsync(TDocument document, string partitionKey = null, CancellationToken cancellationToken = default)
        {
            EntityValidationBase.NotNullOrThrow(document);
            var partition = Schema.ReadAttributeValue<DocumentPartitionAttribute>(document) ?? partitionKey;
            var resolvedPartitionKey = partition is null ? PartitionKey.None : new PartitionKey(partition.ToString());
            return await OperationsExecutor.ExecuteAsync(Container.UpsertItemAsync(document, resolvedPartitionKey, new ItemRequestOptions { EnableContentResponseOnWrite = false }, cancellationToken));
        }

        /// <inheritdoc />
        /// <param name="documents"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override async Task<int> UpdateRangeAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default)
        {
            EntityValidationBase.NotEmptyOrThrow(documents);
            EntityValidationBase.HasNoNullEntityOrThrow(documents);
            EntityValidationBase.HasNoDocumentWithNullableIdOrThrow(documents);
            var documentsGroupedByPartition = new Dictionary<string, IEnumerable<TDocument>>();

            // The cosmos SDK performs better in bulk mode when items to be created share the same partition.
            // Enabling that requires grouping the documents by the partition key and create a batch of execution for each group
            foreach (var document in documents)
            {
                var partitionKey = Schema.ReadAttributeValue<DocumentPartitionAttribute>(document);
                EntityValidationBase.NotNullOrThrow(partitionKey);
                if (documentsGroupedByPartition.ContainsKey(partitionKey.ToString()))
                {
                    documentsGroupedByPartition[partitionKey.ToString()] = documentsGroupedByPartition[partitionKey.ToString()].Append(document);
                }
                else
                {
                    documentsGroupedByPartition.Add(partitionKey.ToString(), new List<TDocument> { document });
                }
            }

            List<Func<Task>> tasksPerPartition = new();
            int numberOfSuccessfulOperations = 0;

            // Go through each group and create delegates to execute
            foreach (var documentGroup in documentsGroupedByPartition)
            {
                var documentsInPartition = documentGroup.Value;
                Task func() => Task.Run(async () =>
                {
                    List<Func<Task>> partitionTasks = new(documentsInPartition.Count());
                    foreach (var document in documentsInPartition)
                    {
                        Task documentTask() => Task.Run(async () =>
                        {
                            await OperationsExecutor.ExecuteAsync(Container.UpsertItemAsync(document, new PartitionKey(documentGroup.Key), new ItemRequestOptions { EnableContentResponseOnWrite = false }, cancellationToken));
                            Interlocked.Increment(ref numberOfSuccessfulOperations);
                        });
                        partitionTasks.Add(documentTask);
                    }
                    await partitionTasks.ExecuteInParallelAsync(_configurationOptions.BulkBatchSize);
                });
                tasksPerPartition.Add(func);
            }
            await tasksPerPartition.ExecuteInParallelAsync();
            return numberOfSuccessfulOperations;
        }
        
        /// <param name="document"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="partitionKey"></param>
        /// <inheritdoc />
        /// <exception cref="CosmosException"></exception>
        public override async Task<TDocument> RemoveAsync(TDocument document, string partitionKey = null, CancellationToken cancellationToken = default)
        {
            EntityValidationBase.NotNullOrThrow(document);
            var entityId = Schema.ReadAttributeValue<DocumentKeyAttribute>(document);
            EntityValidationBase.NotNullOrThrow(entityId);
            var partition = Schema.ReadAttributeValue<DocumentPartitionAttribute>(document) ?? partitionKey;
            var resolvedPartitionKey = partition is null ? PartitionKey.None : new PartitionKey(partition.ToString());
            await OperationsExecutor.ExecuteAsync(Container.DeleteItemAsync<TDocument>(entityId.ToString(), resolvedPartitionKey, new ItemRequestOptions { EnableContentResponseOnWrite = false }, cancellationToken));
            return document;
        }

        /// <inheritdoc />
        /// <param name="documents"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="CosmosException"></exception>
        public override async Task<int> RemoveRangeAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default)
        {
            EntityValidationBase.NotNullOrThrow(documents);
            EntityValidationBase.HasNoNullEntityOrThrow(documents);
            EntityValidationBase.HasNoDocumentWithNullableIdOrThrow(documents);
            var documentsGroupedByPartition = new Dictionary<string, IEnumerable<TDocument>>();

            // The cosmos SDK performs better in bulk mode when items to be created share the same partition.
            // Enabling that requires grouping the documents by the partition key and create a batch of execution for each group
            foreach (var document in documents)
            {
                var partitionKey = Schema.ReadAttributeValue<DocumentPartitionAttribute>(document);
                EntityValidationBase.NotNullOrThrow(partitionKey);
                if (documentsGroupedByPartition.ContainsKey(partitionKey.ToString()))
                {
                    documentsGroupedByPartition[partitionKey.ToString()] = documentsGroupedByPartition[partitionKey.ToString()].Append(document);
                }
                else
                {
                    documentsGroupedByPartition.Add(partitionKey.ToString(), new List<TDocument> { document });
                }
            }

            List<Func<Task>> tasksPerPartition = new();
            int numberOfSuccessfulOperations = 0;

            // Go through each group and create delegates to execute
            foreach (var documentGroup in documentsGroupedByPartition)
            {
                var documentsInPartition = documentGroup.Value;
                Task func() => Task.Run(async () =>
                {
                    List<Func<Task>> partitionTasks = new(documentsInPartition.Count());
                    foreach (var document in documentsInPartition)
                    {
                        Task documentTask() => Task.Run(async () =>
                        {
                            var entityId = Schema.ReadAttributeValue<DocumentKeyAttribute>(document);
                            await OperationsExecutor.ExecuteAsync(Container.DeleteItemAsync<TDocument>(entityId.ToString(), new PartitionKey(documentGroup.Key), new ItemRequestOptions { EnableContentResponseOnWrite = false }, cancellationToken));
                            Interlocked.Increment(ref numberOfSuccessfulOperations);
                        });
                        partitionTasks.Add(documentTask);
                    }
                    await partitionTasks.ExecuteInParallelAsync(_configurationOptions.BulkBatchSize);
                });
                tasksPerPartition.Add(func);
            }
            await tasksPerPartition.ExecuteInParallelAsync();
            return numberOfSuccessfulOperations;
        }
    }
}
