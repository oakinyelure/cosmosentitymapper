using Cosmos.Entity.Mapper.Schema;
using Microsoft.Azure.Cosmos;
using System;

namespace Cosmos.Entity.Mapper
{
    /// <summary>
    /// A CosmosDbContext instance represents an instance of a cosmos database. it can be used to access all
    /// documents in the database using a combination of unit of work and repository patterns
    /// </summary>
    /// <remarks>
    /// <para>
    /// An instance of the <see cref="CosmosDbContext"/> can be used in multiple parallel operations. Each Collection has a new instance of 
    /// associated collection and can be used in parallel
    /// </para>
    /// <para>
    /// Like most entity mappers, you need to create a class that extends the <see cref="CosmosDbContext"/>. The class must contain properties of type <see cref="CollectionSet{TEntity}"/>
    /// This will give you access to the repository
    /// </para>
    /// <para>
    /// Each collection is automatically initialized so you won't need to new them up
    /// </para>
    /// <para>No migration, scaffolding or configuration is needed.</para>
    /// </remarks>
    public abstract class CosmosDbContext : IDisposable
    {
        /// <summary>
        /// Instance of the cosmos database
        /// </summary>
        /// <remarks>
        /// <para>
        /// Exposes all Cosmosb DB public API. This allows you to use the native SDK, run query and other operations on the database
        /// </para>
        /// </remarks>
        public Database Database { get; internal set; }

        /// <summary>
        /// Interact directly with the CosmosDB client
        /// </summary>
        public CosmosClient Client { get; internal set; }

        /// <summary>
        /// Configures the <see cref="CosmosDbContext"/> using <see cref="ContextOptionsBase"/>
        /// </summary>
        /// <param name="options"></param>
        protected CosmosDbContext(ContextOptionsBase options)
        {
            options.ConfigureContext(this);
            new CollectionSetInitializer().InitializeCollectionSet(this, Database, options);
        }

        /// <summary>
        /// Configures instance of the <see cref="CosmosDbContext" /> using <paramref name="connectionString"/> and default configuration options
        /// </summary>
        /// <remarks>
        /// <para>The database name can either be in the <paramref name="connectionString"/> or added using the <see cref="DatabaseAttribute"/></para>
        /// </remarks>
        /// <param name="connectionString">
        /// <para><paramref name="connectionString"/> Must contain a typical cosmos db connection string. This should include the AccountName and Account Key at the least</para>
        /// </param>
        protected CosmosDbContext(string connectionString) 
        {
            ContextOptionsBase contextOption = new ContextOptions { ConnectionString = connectionString };
            contextOption.ConfigureContext(this);
            var collectionPropertyInitializer = new CollectionSetInitializer();
            collectionPropertyInitializer.InitializeCollectionSet(this, Database, contextOption);
        }

        /// <summary>
        /// Performs cleanup of used resources
        /// </summary>
        public virtual void Dispose()
        {
            Client?.Dispose();            
        }
    }
}
 