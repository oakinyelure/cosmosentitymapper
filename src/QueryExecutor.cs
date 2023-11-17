using Cosmos.Entity.Mapper.Diagnostics.Logging;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace Cosmos.Entity.Mapper
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class QueryExecutor<TEntity> : IQueryExecutor<TEntity>
    {
        private readonly IDiagnosticsLogger _diagnostics;

        /// <summary>
        /// Creates instance of the operation executor
        /// </summary>
        public QueryExecutor()
        {
            _diagnostics = new DiagnosticsLogger();
        }

        /// <inheritdoc />
        public async Task<ItemResponse<TEntity>> ExecuteAsync(Task<ItemResponse<TEntity>> operation)
        {
            try
            {
                
                var operationsResult = await operation;
                _diagnostics.LogOperationsResult(operationsResult);
                return operationsResult;
            }
            catch(CosmosException ex)
            {
                _diagnostics.LogCosmosException(ex);
                throw;
            }
        }



        /// <inheritdoc />
        public async Task<FeedResponse<TEntity>> ExecuteAsync(Task<FeedResponse<TEntity>> operation)
        {
            try
            {
                var operationResult = await operation;
                _diagnostics.LogOperationsResult(operationResult);
                return operationResult;
            }
            catch(CosmosException ex) 
            {
                _diagnostics.LogCosmosException(ex);
                throw;
            }
        }

        /// <inheritdoc />
        /// <exception cref="CosmosException"></exception>
        public async Task<ResponseMessage> ExecuteAsync(Task<ResponseMessage> operation)
        {
            try
            {
                var operationResult = await operation;
                if(!operationResult.IsSuccessStatusCode)
                {
                    operationResult.Headers.TryGetValue("x-ms-request-charge", out var requestCharge);
                    double.TryParse(requestCharge, out var requestChargeValue);
                    throw new CosmosException(operationResult.ErrorMessage, operationResult.StatusCode, 0, operationResult.Headers.ActivityId, requestChargeValue);
                }
                _diagnostics.LogOperationsResult<TEntity>(operationResult);
                return operationResult;
            }
            catch (CosmosException ex)
            {
                _diagnostics.LogCosmosException(ex);
                throw;
            }
        }
    }
}
