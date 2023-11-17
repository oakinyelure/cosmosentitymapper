using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace Cosmos.Entity.Mapper.Diagnostics.Logging
{
    /// <summary>
    ///     This is an internal API that supports the Cosmos Entity Mapper infrastructure and not subject to the 
    ///     same compatibility standard as public APIS. It may be changed or removed without notive in any release
    /// </summary>
    public class DiagnosticsLogger : IDiagnosticsLogger
    {
        private static ILoggerFactory _loggerFactory = null;

        private static ILogger GetLogger() => LoggerFactory.CreateLogger<DiagnosticsLogger>();

        /// <summary>
        /// Logger factory
        /// </summary>
        public static ILoggerFactory LoggerFactory
        {                      
            get
            {
                if(_loggerFactory is null)
                {
                    var serviceCollection = new ServiceCollection();                    
                    serviceCollection.AddLogging(builder => 
                        builder
                        .AddDebug()
                        .AddConsole()                      
                    );
                    _loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
                }
                return _loggerFactory;
            }
            set
            {
                _loggerFactory = value;
            }
        }

        /// <inheritdoc />
        public DiagnosticsLogger LogOperationsResult<TEntity>(ItemResponse<TEntity> operationsResult)
        {
            dynamic diagnosticResult = new 
            { 
                operationsResult.StatusCode,
                Cost = operationsResult.RequestCharge,
                NumberOfFailures = operationsResult.Diagnostics?.GetFailedRequestCount(),
                ExecutionTime = operationsResult.Diagnostics?.GetClientElapsedTime().Milliseconds,
                RegionsContacted = operationsResult.Diagnostics is not null ? string.Join(",",operationsResult.Diagnostics.GetContactedRegions().Select(region => region.regionName)) : null,
                PayloadSize = operationsResult.Headers?.ContentLength                
            };
            string jsonResult = JsonSerializer.Serialize(diagnosticResult, new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true });
            LogLevel level = LogLevel.Debug;
            level = !new HttpResponseMessage(operationsResult.StatusCode).IsSuccessStatusCode ? LogLevel.Error : level;
            Log(level, jsonResult);
            return this;
        }

        /// <inheritdoc />
        public DiagnosticsLogger Log(LogLevel level, string message,params object[] args)
        {
            GetLogger().Log(level, message,args);
            return this;
        }

        /// <inheritdoc />
        public DiagnosticsLogger LogCosmosException(CosmosException exception)
        {
            dynamic diagnosticResult = new
            {
                exception.StatusCode,
                Cost = exception.RequestCharge,
                exception.Message,
                NumberOfFailures = exception.Diagnostics?.GetFailedRequestCount(),
                ExecutionTime = exception.Diagnostics?.GetClientElapsedTime().Milliseconds,
                RegionsContacted = exception.Diagnostics is not null ? string.Join(",", exception.Diagnostics.GetContactedRegions().Select(region => region.regionName)) : null,
                PayloadSize = exception.Headers?.ContentLength
            };
            string jsonResult = JsonSerializer.Serialize(diagnosticResult, new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true });
            Log(LogLevel.Error, jsonResult);
            return this;
        }

        /// <inheritdoc />
        public DiagnosticsLogger LogOperationsResult<TEntity>(FeedResponse<TEntity> operationsResult)
        {
            dynamic diagnosticResult = new
            {
                operationsResult.StatusCode,
                Cost = operationsResult.RequestCharge,
                NumberOfFailures = operationsResult.Diagnostics?.GetFailedRequestCount(),
                ExecutionTime = operationsResult.Diagnostics?.GetClientElapsedTime().Milliseconds,
                RegionsContacted = operationsResult.Diagnostics is not null ? string.Join(",", operationsResult.Diagnostics.GetContactedRegions().Select(region => region.regionName)) : null,
                PayloadSize = operationsResult.Headers?.ContentLength
            };
            string jsonResult = JsonSerializer.Serialize(diagnosticResult, new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true });
            Log(LogLevel.Debug, jsonResult);
            return this;
        }

        /// <inheritdoc />
        public DiagnosticsLogger LogOperationsResult<TEntity>(ResponseMessage operationsResult)
        {
            bool hasRequestCharge = operationsResult.Headers.TryGetValue("x-ms-request-charge", out var requestCost);
            dynamic diagnosticResult = new
            {
                operationsResult.StatusCode,
                Cost = hasRequestCharge ? requestCost : null,
                NumberOfFailures = operationsResult.Diagnostics?.GetFailedRequestCount(),
                ExecutionTime = operationsResult.Diagnostics?.GetClientElapsedTime().Milliseconds,
                RegionsContacted = operationsResult.Diagnostics is not null ? string.Join(",", operationsResult.Diagnostics.GetContactedRegions().Select(region => region.regionName)) : null,
                PayloadSize = operationsResult.Headers?.ContentLength
            };
            string jsonResult = JsonSerializer.Serialize(diagnosticResult, new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true });
            Log(LogLevel.Debug, jsonResult);
            return this;
        }
    }
}
