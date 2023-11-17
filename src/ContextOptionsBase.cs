namespace Cosmos.Entity.Mapper
{
    using Microsoft.Azure.Cosmos;
    using System.Data.Common;
    using System;
    using Cosmos.Entity.Mapper.Schema;

    /// <summary>
    /// A base class for configuring <see cref="CosmosDbContext"/> options
    /// </summary>
    public abstract class ContextOptionsBase : CosmosClientOptions
    {
        /// <summary>
        /// The connection string to the database.
        /// </summary>
        /// <remarks>
        /// <para>This is a typical CosmosDb connection string with AccountEndpoint,Account Key and Database. This database is optional but must be defined in the schema of the <see cref="CosmosDbContext"/></para>
        /// </remarks>
        public virtual string ConnectionString { get; set; }

        /// <summary>
        /// Configures the batch size of each transaction when in bulk mode
        /// </summary>
        public virtual int BulkBatchSize { get; set; } = 100;

        internal virtual void ConfigureContext(CosmosDbContext context)
        {
            if(string.IsNullOrEmpty(ConnectionString) || string.IsNullOrWhiteSpace(ConnectionString)) throw new ArgumentException($"{nameof(ConnectionString)} cannot be null or empty");
            base.AllowBulkExecution = AllowBulkExecution;
            DbConnectionStringBuilder connectionStringBuilder = new() { ConnectionString = ConnectionString };
            EnforceMinimumConnectionStringRequirementAndThrow(connectionStringBuilder,context);
            string connectionString = string.Format("AccountEndpoint={0};AccountKey={1}", connectionStringBuilder["AccountEndpoint"], connectionStringBuilder["AccountKey"]);
            connectionStringBuilder.TryGetValue("ApplicationName", out var applicationName);
            if (applicationName is not null) ApplicationName = applicationName.ToString();
            context.Client = new CosmosClient(connectionString, this);
            context.Database = context.Client.GetDatabase(connectionStringBuilder["Database"].ToString());
        }

        /// <summary>
        /// Checks a CosmosDb ConnectionString to evaluate the minimum requirements for a successful connection
        /// </summary>
        /// <param name="connectionStringBuilder"></param>
        /// <param name="context"></param>
        public virtual void EnforceMinimumConnectionStringRequirementAndThrow(DbConnectionStringBuilder connectionStringBuilder, CosmosDbContext context)
        {
            var schemaReader = SchemaDefinitionReader<CosmosDbContext>.GetInstanceFromType(context.GetType());
            var databaseSchema = schemaReader.GetAttributeInstanceFromType<DatabaseAttribute>();
            connectionStringBuilder.TryGetValue("Database",out var databaseName);
            databaseName ??= databaseSchema?.DatabaseName;
            if (!connectionStringBuilder.ContainsKey("AccountEndpoint") || !connectionStringBuilder.ContainsKey("AccountKey") || databaseName is null) throw new ArgumentException($"{nameof(connectionStringBuilder.ConnectionString)} does not pass minimum requirement");
            connectionStringBuilder["Database"] = databaseName;
        }
    }
}
