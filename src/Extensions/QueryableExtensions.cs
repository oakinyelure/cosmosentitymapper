using Cosmos.Entity.Mapper.Utilities;
using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

        /// <summary>
        /// Performs a dynamic order by on <paramref name="property"/> using <paramref name="direction"/> to determine the sort order
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static IOrderedQueryable<TDocument> OrderByDynamicProperty<TDocument>(this IQueryable<TDocument> source, string property, ListSortDirection? direction = null)
        {
            EntityValidationBase.NotNullOrThrow(source);
            EntityValidationBase.NotNullOrThrow(property);
            var propertyInstance = typeof(TDocument).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(e => e.Name.Equals(property, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault() ?? throw new InvalidOperationException($"{property} is not supported");
            var leftParam = Expression.Parameter(source.ElementType, "arg");
            MemberExpression memberExpression = Expression.Property(leftParam, propertyInstance.Name);
            string sortmethod = direction is not null ? (direction == ListSortDirection.Ascending ? "OrderBy" : "OrderByDescending") : "OrderBy";
            MethodCallExpression expression = Expression.Call(typeof(Queryable), sortmethod, new Type[] { source.ElementType, memberExpression.Type }, source.Expression, Expression.Quote(Expression.Lambda(memberExpression, leftParam)));
            return source.Provider.CreateQuery(expression) as IOrderedQueryable<TDocument>;
        }
    }
}
