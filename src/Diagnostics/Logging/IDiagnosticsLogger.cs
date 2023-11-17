using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Cosmos.Entity.Mapper.Diagnostics.Logging
{
    /// <summary>
    /// Contract for the internal diagnostics logger
    /// </summary>
    public interface IDiagnosticsLogger
    {
        /// <summary>
        /// Logs an operations result from <see cref="ItemResponse{T}"/> using configured logging providers
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="operationsResult"></param>
        /// <returns></returns>
        DiagnosticsLogger LogOperationsResult<TEntity>(ItemResponse<TEntity> operationsResult);

        /// <summary>
        /// Logs an operations result from <see cref="FeedResponse{T}"/>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="operationsResult"></param>
        /// <returns></returns>
        DiagnosticsLogger LogOperationsResult<TEntity>(FeedResponse<TEntity> operationsResult);

        /// <summary>
        /// Logs the result of an operation returning a <see cref="ResponseMessage" />
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="operationsResult"></param>
        /// <returns></returns>
        DiagnosticsLogger LogOperationsResult<TEntity>(ResponseMessage operationsResult);

        /// <summary>
        /// Logs a message using the configured logging providers
        /// </summary>
        /// <remarks>
        /// <para>This is an internal API and not subject to public use. </para>
        /// </remarks>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns><see cref="DiagnosticsLogger"/> to allow method chaining</returns>
        DiagnosticsLogger Log(LogLevel level, string message, params object[] args);

        /// <summary>
        /// Logs a <see cref="CosmosException"/> object in the form
        /// </summary>
        ///         /// <remarks>
        /// <para>This is an internal API and not subject to public use. </para>
        /// </remarks>
        /// <param name="exception"></param>
        DiagnosticsLogger LogCosmosException(CosmosException exception);        
    }
}
