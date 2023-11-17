using Microsoft.Extensions.DependencyInjection;

namespace Cosmos.Entity.Mapper.Extensions
{
    /// <summary>
    /// <see cref="Mapper"/> specific extension methods for <see cref="IServiceCollection"/>
    /// </summary>
    public static class CosmosEntityMapperServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the given cosmos entity mapper <typeparamref name="TContext"/> as a service in the <see cref="IServiceCollection"/>
        /// and configures it to connect to the instance of the Cosmos Db using the <see cref="CosmosDbContext.CosmosDbContext(ContextOptionsBase)"/> constructor
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddCosmosEntityMapper<TContext>(this IServiceCollection services, ContextOptionsBase options)
            where TContext : CosmosDbContext
        {
            return services.AddSingleton<TContext>(_ => ActivatorUtilities.CreateInstance<TContext>(_,options));
        }

        /// <summary>
        /// Registers the given cosmos entity mapper <typeparamref name="TContext"/> as a service in the <see cref="IServiceCollection"/>
        /// and configures it to connect to the instance of the Cosmos Db using the <see cref="CosmosDbContext.CosmosDbContext(string)"/> constructor
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IServiceCollection AddCosmosEntityMapper<TContext>(this IServiceCollection services, string connectionString)
            where TContext : CosmosDbContext
        {
            return services.AddSingleton<TContext>(_ => ActivatorUtilities.CreateInstance<TContext>(_, new ContextOptions { ConnectionString = connectionString }));
        }
    }
}
