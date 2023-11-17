using System.Linq;

namespace Cosmos.Entity.Mapper.Extensions
{
    /// <summary>
    /// Extension methods specific to IQueryables
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Adds a pagination using an offset like a traditional RDMS would
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <remarks>
        /// This approach is not recommended per Microsoft as it is prone to some issues. It should not be used where all these factors may apply
        /// <para>Ordering must be unique to get the best out of pagination. Might not get consistent result if entity identity is not properly designed</para>
        /// <para>Concurrent updates in the database will be skipped or might get included in a different page</para>
        /// </remarks>
        /// <returns></returns>
        public static IQueryable<TDocument> PaginateByOffset<TDocument>(this IOrderedQueryable<TDocument> query, int? page = 0, int? pageSize = 50)
        {
            byte pageOffset = 1;
            return query.Skip(((int)page - pageOffset) * (int)pageSize)
                .Take((int)pageSize);
        }

        /// <summary>
        /// Adds a pagination using an offset like a traditional RDMS would
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <remarks>
        /// This approach is not recommended per Microsoft as it is prone to some issues. It should not be used where all these factors may apply
        /// <para>Ordering must be unique to get the best out of pagination. Might not get consistent result if entity identity is not properly designed</para>
        /// <para>Concurrent updates in the database will be skipped or might get included in a different page</para>
        /// </remarks>
        /// <returns></returns>
        public static IQueryable<TDocument> PaginateByOffset<TDocument>(this IQueryable<TDocument> query, int? page = 1, int? pageSize = 50)
        {
            byte pageOffset = 1;
            return query.Skip(((int)page - pageOffset) * (int)pageSize)
                .Take((int)pageSize);
        }
    }
}
