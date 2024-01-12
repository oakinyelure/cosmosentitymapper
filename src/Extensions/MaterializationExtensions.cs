using Microsoft.Azure.Cosmos;
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
    /// Cosmos Entity Mapper related extensions
    /// </summary>
    public static class MaterializationExtensions
    {
        /// <summary>
        /// Provides a facade or an interface to actualize the query.
        /// </summary>
        /// <remarks>
        /// <para>This was added to prevent conflict in code base using EFCore because they share a lot of APIs </para>
        /// </remarks>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryMaterializable<TDocument> AsMaterializable<TDocument>(this IQueryable<TDocument> source)
        {
            return new QueryMaterializer<TDocument>(source, new QueryExecutor<TDocument>());
        }

        /// <summary>
        /// Actualizes the <see cref="IQueryable{T}"/> to an enumerable and returns a list 
        /// of <typeparamref name="TDocument"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<TDocument> AsEnumerableAsync<TDocument>(this IQueryable<TDocument> source,[EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach(var item in AsMaterializable(source).AsEnumerableAsync(cancellationToken))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Materializes the <see cref="IQueryExecutor{TDocument}"/> to a list
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static async Task<List<TDocument>> ToListAsync<TDocument>(this IQueryable<TDocument> source, CancellationToken cancellationToken = default)
            => await AsMaterializable(source).ToListAsync(cancellationToken);


        /// <summary>
        /// Uses the feed iterator to get the first element in the sequence. Returns null if no item
        /// is in the sequence
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TDocument> FirstOrDefaultAsync<TDocument>(this IQueryable<TDocument> source, Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default)
            => await source.AsMaterializable().FirstOrDefaultAsync(predicate, cancellationToken);

        /// <summary>
        /// Like <see cref="FirstOrDefaultAsync"/>, this returns the first element in the sequence. However it throws a
        /// <see cref="InvalidOperationException"/> exception if no item is in the sequence
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="source"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static async Task<TDocument> FirstAsync<TDocument>(this IQueryable<TDocument> source, Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default)
            => await source.AsMaterializable().FirstAsync(predicate, cancellationToken);

        /// <summary>
        /// Like <see cref="FirstAsync"/>, <see cref="SingleAsync"/> returns the first element in a sequence but throws an <see cref="InvalidOperationException"/> exception
        /// if there are no element or if there more than one item in the sequence
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="predicate"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static async Task<TDocument> SingleAsync<TDocument>(this IQueryable<TDocument> source, Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default)
            => await source.AsMaterializable().SingleAsync(predicate, cancellationToken);

        /// <summary>
        /// Like <see cref="FirstOrDefaultAsync"/>, this also returns the first element in the sequence or null if no item is in the sequence
        /// <para>However, will throw <see cref="InvalidOperationException"/> if there are more than one item in the sequence</para>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="predicate"></param>
        /// <param name="source"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static async Task<TDocument> SingleOrDefaultAsync<TDocument>(this IQueryable<TDocument> source, Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default)
            => await source.AsMaterializable().SingleOrDefaultAsync(predicate, cancellationToken);

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
        /// <param name="source"></param>
        /// <returns></returns>
        public static async Task<int> CountAsync<TDocument>(this IQueryable<TDocument> source, Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default)
            => await source.AsMaterializable().CountAsync(predicate, cancellationToken);

        /// <summary>
        /// Checks if any document is in the sequence.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="source"></param>
        /// <returns>Returns true if there is at least one document in the sequence. Returns false otherwise</returns>
        public static async Task<bool> AnyAsync<TDocument>(this IQueryable<TDocument> source, Expression<Func<TDocument, bool>> predicate = null, CancellationToken cancellationToken = default)
            => await source.AsMaterializable().AnyAsync(predicate, cancellationToken);

        /// <summary>
        /// Checks if all documents in the sequence passes the test in the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="source"></param>
        /// <returns>
        /// <para>A task representing the asynchronous result. Would return <see langword="true"/> if every document in the sequence matches the predicate</para>
        /// </returns>
        public static async Task<bool> AllAsync<TDocument>(this IQueryable<TDocument> source, Expression<Func<TDocument, bool>> predicate, CancellationToken cancellationToken = default)
            => await source.AsMaterializable().AllAsync(predicate, cancellationToken);
    }
}
