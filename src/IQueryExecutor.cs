using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace Cosmos.Entity.Mapper
{
    /// <summary>   
    /// <para>
    /// <see cref="ExecuteAsync(Task{ItemResponse{TEntity}})"/> executes the Cosmos operation and catches any error that may occur and attempts
    /// to log it. This is to make sure errors from cosmos related operations are captured
    /// </para>
    /// </summary>
    public interface IQueryExecutor<TEntity>
    {
        /// <summary>
        /// Executes an operation that expects a <see cref="ItemResponse{T}"/>
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        Task<ItemResponse<TEntity>> ExecuteAsync(Task<ItemResponse<TEntity>> operation);

        /// <summary>
        /// Executes a task that returns a <see cref="FeedResponse{T}"/>
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        Task<FeedResponse<TEntity>> ExecuteAsync(Task<FeedResponse<TEntity>> operation);

        /// <summary>
        /// Executes a task that returns a <see cref="ResponseMessage"/>
        /// </summary>
        /// <param name="operation"></param>
        /// <returns>The result of <paramref name="operation"/></returns>
        Task<ResponseMessage> ExecuteAsync(Task<ResponseMessage> operation);
    }
}
