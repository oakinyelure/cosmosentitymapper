using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Reflection;

namespace Cosmos.Entity.Mapper
{
    internal interface ICollectionSetInitializer
    {
        /// <summary>
        /// Initializes all <see cref="CollectionSet{TEntity}"/> properties within a <see cref="CosmosDbContext"/>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="database"></param>
        /// <param name="options"></param>
        void InitializeCollectionSet(CosmosDbContext context, Database database, ContextOptionsBase options);

        /// <summary>
        /// Returns all properties defined in the <see cref="CosmosDbContext"/> that can be initialized as a CollectionSet
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IEnumerable<PropertyInfo> GetCollectionSetFromContext(CosmosDbContext context);
    }
}
