using Cosmos.Entity.Mapper.Utilities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.Entity.Mapper
{
    /// <summary>
    /// Cosmos SDK <see cref="Container.GetItemLinqQueryable{T}(bool, string, QueryRequestOptions, CosmosLinqSerializerOptions)"/> returns an <see cref="IQueryable{TEntity>"/>
    /// with no result set. The queryable needs to be converted to a Feed Iterator to traverse through the results as they come through in batches. Callers are able to 
    /// call Queryable extensions on the queryable returned but in a typical enterprise system where EF.Core is in place, there could be a clash as there is an alternative EF Core
    /// Cosmos Db provider. To avoid this, the collection APIs will return <see cref="IQueryMaterializable{TEntity}"/> which will contain methods similar to EFCore extensions that can be
    /// used to actualize the result set
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    public class QueryMaterializer<TDocument> : IQueryMaterializable<TDocument>
    {
        private readonly IQueryExecutor<TDocument> _executor;
        private IQueryable<TDocument> _queryable;

        /// <inheritdoc />
        public QueryMaterializer(IQueryable<TDocument> queryable, IQueryExecutor<TDocument> executor)
        {
            EntityValidationBase.NotNullOrThrow(queryable);
            EntityValidationBase.NotNullOrThrow(executor);
            _executor = executor;
            _queryable = queryable;

        }

        /// <inheritdoc />
        public async IAsyncEnumerable<TDocument> AsEnumerableAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var iterator = ToFeedIterator();
            while (iterator.HasMoreResults)
            {
                var collection = await _executor.ExecuteAsync(iterator.ReadNextAsync(cancellationToken));
                if (collection is null) continue;
                foreach (var item in collection)
                {
                    yield return item;
                }
            }
        }

        /// <inheritdoc />
        public async virtual Task<TDocument> FirstOrDefaultAsync(Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default)
        {            
            if (predicate is not null)
            {
                _queryable = _queryable.Where(predicate);
            }
            _queryable = _queryable.Take(1);
            var resultSet = await ToListAsync(cancellationToken);
            return resultSet.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<TDocument> SingleOrDefaultAsync(Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate is not null)
            {
                _queryable = _queryable.Where(predicate);
            }
            _queryable = _queryable.Take(2);
            var resultSet = await ToListAsync(cancellationToken);
            if (resultSet.Count() > 1) throw new InvalidOperationException("Materialized result contains more than one document");
            return resultSet.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<TDocument> FirstAsync(Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate is not null)
            {
                _queryable = _queryable.Where(predicate);
            }
            _queryable = _queryable.Take(1);
            var resultSet = await ToListAsync(cancellationToken);
            if (resultSet.Count() == 0) throw new InvalidOperationException("No item found in the sequence");
            return resultSet.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<TDocument> SingleAsync(Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate is not null)
            {
                _queryable = _queryable.Where(predicate);
            }
            _queryable = _queryable.Take(2);
            var resultSet = await ToListAsync(cancellationToken);
            if (resultSet.Count() > 1) throw new InvalidOperationException("Materialized result contains more than one document");
            if (!resultSet.Any()) throw new InvalidOperationException("No item found in the sequence");
            return resultSet.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate is not null)
            {
                _queryable = _queryable.Where(predicate);
            }
            return await _queryable.CountAsync(cancellationToken);
        }



        /// <inheritdoc/>
        public virtual FeedIterator<TDocument> ToFeedIterator()
        {
            return _queryable.ToFeedIterator();
        }

        /// <inheritdoc />
        public async Task<List<TDocument>> ToListAsync(CancellationToken cancellationToken = default)
        {
            List<TDocument> results = new();
            await foreach (var item in AsEnumerableAsync(cancellationToken))
            {
                results.Add(item);
            }
            return results;
        }

        /// <inheritdoc />
        public async Task<bool> AnyAsync(Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            var matchingElement = await FirstOrDefaultAsync(predicate, cancellationToken);
            return matchingElement is not null;
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para><see cref="AllAsync(Expression{Func{TDocument, bool}}, CancellationToken)"/> have not been publicly released. Internal implementation have not been fully tested and should
        /// not be used in a production environment. Updates can be made to the method without public acknowledgement</para>
        /// </remarks>
        public async Task<bool> AllAsync(Expression<Func<TDocument, bool>> predicate, CancellationToken cancellationToken = default)
        {
            EntityValidationBase.NotNullOrThrow(predicate);
            // Negate the predicate to query only documents that does not match. We don't expect a match as every document in the sequence is expected to match
            Expression<Func<TDocument,bool>> invertedPredicate = Expression.Lambda<Func<TDocument,bool>>(Expression.Not(predicate.Body), predicate.Parameters);
            var firstNonMatchingItem = await FirstOrDefaultAsync(invertedPredicate, cancellationToken);
            return firstNonMatchingItem is null;
        }
    }
}
