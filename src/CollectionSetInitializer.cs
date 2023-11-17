namespace Cosmos.Entity.Mapper
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using Microsoft.Azure.Cosmos;
    using Cosmos.Entity.Mapper.Internal;
    using Cosmos.Entity.Mapper.Schema;

    /// <summary>
    /// An internal API to set instance of a collection set
    /// </summary>
    public class CollectionSetInitializer : ICollectionSetInitializer
    {
        /// <inheritdoc />
        public void InitializeCollectionSet(CosmosDbContext context, Database database, ContextOptionsBase options)
        {
            var collectionsInContext = GetCollectionSetFromContext(context).Where(property => property.GetValue(context) is null).ToList();
            collectionsInContext.ForEach(property =>
                {
                    var collectionSet = typeof(InternalCollectionSet<>).MakeGenericType(property.PropertyType.GenericTypeArguments);

                    // A collection set should only have one parameter. Let's get the first one
                    var schemaReader = SchemaDefinitionReader<object>.GetInstanceFromType(property.PropertyType.GenericTypeArguments.First());
                    string collectionName = schemaReader?.GetAttributeInstanceFromType<CollectionAttribute>()?.Name ?? property.Name;
                    var propertyOnContext = context.GetType().GetProperty(property.Name);
                    var collection = Activator.CreateInstance(collectionSet, database.GetContainer(collectionName),options);
                    propertyOnContext.SetValue(context, collection);
                });
        }

        /// <inheritdoc />
        public IEnumerable<PropertyInfo> GetCollectionSetFromContext(CosmosDbContext context)
        {
            return context.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(entity => entity.PropertyType.IsGenericType && entity.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(CollectionSet<>)));
        }
    }
}
