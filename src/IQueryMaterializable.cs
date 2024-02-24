using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.Entity.Mapper
{
    /// <summary>
    /// Cosmos SDK <see cref="Container.GetItemLinqQueryable{T}(bool, string, QueryRequestOptions, CosmosLinqSerializerOptions)"/> returns an <see cref="IQueryable{TDocument>"/>
    /// with no result set. The queryable needs to be converted to a Feed Iterator to traverse through the results as they come through in batches. Callers are able to 
    /// call Queryable extensions on the queryable returned but in a typical enterprise system where EF.Core is in place, there could be a clash as there is an alternative EF Core
    /// Cosmos Db provider. To avoid this, the collection APIs will return <see cref="IQueryMaterializable{TEntity}"/> which will contain methods similar to EFCore extensions that can be
    /// used to actualize the result set
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    public interface IQueryMaterializable<TDocument>
    {

        /// <summary>
        /// Converts the queryable to a Feed Iterator that can be used by the Cosmos Db SDK
        /// </summary>
        /// <returns></returns>
        FeedIterator<TDocument> ToFeedIterator();

        /// <summary>
        /// Provides a non-thread-blocking interface to materialize the queryable
        /// of <typeparamref name="TDocument"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IAsyncEnumerable<TDocument> AsEnumerableAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Materializes the <see cref="IQueryExecutor{TDocument}"/> to a list
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<TDocument>> ToListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Uses the feed iterator to get the first element in the sequence. Returns null if no item
        /// is in the sequence
        /// </summary>
        /// <returns></returns>
        Task<TDocument> FirstOrDefaultAsync(Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Like <see cref="FirstOrDefaultAsync"/>, this also returns the first element in the sequence or null if no item is in the sequence
        /// <para>However, will throw <see cref="InvalidOperationException"/> if there are more than one item in the sequence</para>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="predicate"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        Task<TDocument> SingleOrDefaultAsync(Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Like <see cref="FirstOrDefaultAsync"/>, this returns the first element in the sequence. However it throws a
        /// <see cref="InvalidOperationException"/> exception if no item is in the sequence
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        Task<TDocument> FirstAsync(Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Like <see cref="FirstAsync"/>, <see cref="SingleAsync"/> returns the first element in a sequence but throws an <see cref="InvalidOperationException"/> exception
        /// if there are no element or if there more than one item in the sequence
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<TDocument> SingleAsync(Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously gets the count of item in a sequence.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Like all the other query actualization method, it relies on the <see cref="FeedIterator"/>
        /// </para>
        /// </remarks>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> CountAsync(Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if any document is in the sequence.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns true if there is at least one document in the sequence. Returns false otherwise</returns>
        Task<bool> AnyAsync(Expression<Func<TDocument,bool>> predicate = null, CancellationToken cancellationToken= default);

        /// <summary>
        /// Checks if all documents in the sequence passes the test in the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// <para>A task representing the asynchronous result. Would return <see langword="true"/> if every document in the sequence matches the predicate</para>
        /// </returns>
        Task<bool> AllAsync(Expression<Func<TDocument, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
