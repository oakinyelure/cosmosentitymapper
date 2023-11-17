using Cosmos.Entity.Mapper.Schema;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.Entity.Mapper
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    public abstract class CollectionSet<TDocument> : IQueryable<TDocument>
        where TDocument : class
    {
        /// <summary>
        /// The Id of the container
        /// </summary>
        /// <remarks>
        /// <para>This can be also refer to the collection or container name</para>
        /// </remarks>
        public string CollectionId { get; private set; }

        /// <summary>
        /// The container object the collection will access
        /// </summary>
        public Container Container { get; private set; }

        /// <summary>
        /// Holds an instance of the operations executor. 
        /// </summary>
        protected IQueryExecutor<TDocument> OperationsExecutor { get; private set; }

        /// <summary>
        /// Sets the schema definition instance reader used by the collectionset
        /// </summary>
        protected ISchemaDefinitionReader<TDocument> Schema { get; private set; }

        /// <summary>
        /// Create instance of the the collection set and attach its container
        /// </summary>
        protected CollectionSet(Container container)
        {
            Container = container;
            CollectionId = container.Id;
            OperationsExecutor = new QueryExecutor<TDocument>();
            Schema = new SchemaDefinitionReader<TDocument>();
        }

        /// <summary>
        /// Projects the current instance to another type. 
        /// </summary>        
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract CollectionSet<T> OfType<T>() where T : class;

        /// <summary>
        /// Finds a document from an entity store using the <paramref name="id"/> and the <paramref name="partitionKey"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partitionKey"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="CosmosException" />
        /// <returns>Document matching the <paramref name="id"/> and the <paramref name="partitionKey"/></returns>
        public abstract Task<TDocument> FindAsync(string id, string partitionKey = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds <paramref name="entity"/> to the <see cref="Container"/>
        /// </summary>
        /// <param name="entity"></param>.
        /// <param name="partitionKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The added entity</returns>
        public abstract Task<TDocument> AddAsync(TDocument entity, string partitionKey = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a collection of <paramref name="documents"/> to the container
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Number of successful oprations</returns>
        /// <exception cref="CosmosException" />
        public abstract Task<int> AddRangeAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replace or creates an existing entity
        /// </summary>
        /// <remarks>
        /// <para>Uses the upsert functionality of the underlying provider. It uses the unique Id of the document to check if the entity exist. If the entity
        /// exist, it is replaced with <paramref name="document"/></para>. If not, the entity is created
        /// </remarks>
        /// <param name="document"></param>
        /// <param name="partitionKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated entity</returns>
        /// <exception cref="CosmosException" />
        public abstract Task<TDocument> UpdateAsync(TDocument document, string partitionKey = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces many or creates many documents if they don't exist
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="CosmosException" />
        /// <returns>The number of documents affected</returns>
        public abstract Task<int> UpdateRangeAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes entity from the container
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="partitionKey"></param>
        /// <returns>Removed entity</returns>
        /// <exception cref="CosmosException" />
        public abstract Task<TDocument> RemoveAsync(TDocument entity, string partitionKey = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes all the entities in the <paramref name="documents"/> from the container
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="CosmosException" />
        /// <returns>Total number of completed </returns>
        public abstract Task<int> RemoveRangeAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public virtual IEnumerator<TDocument> GetEnumerator() => throw new NotImplementedException();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();


        /// <inheritdoc />
        public Type ElementType => throw new NotImplementedException();

        /// <inheritdoc />
        public Expression Expression => throw new NotImplementedException();

        /// <inheritdoc />
        public IQueryProvider Provider => throw new NotImplementedException();
    }
}
