namespace Cosmos.Entity.Mapper.Schema
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Reads the configuration and metadata definition on a collection
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class SchemaDefinitionReader<TEntity> : ISchemaDefinitionReader<TEntity>
    {
        /// <summary>
        /// Returns an instance of <see cref="SchemaDefinitionReader{TEntity}"/> using the type
        /// </summary>
        /// <remarks>
        /// <para>Ideal when the type if generated dynamically</para>
        /// </remarks>
        /// <param name="type"></param>
        public static dynamic GetInstanceFromType(Type type)
        {
            var definitionReaderType = typeof(SchemaDefinitionReader<>).MakeGenericType(type);
            return Activator.CreateInstance(definitionReaderType);
        }

        /// <summary>
        /// Returns an instance of <see cref="SchemaDefinitionReader{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SchemaDefinitionReader<T> GetInstanceFromType<T>(T obj)
        {
            if (!typeof(T).IsAssignableFrom(obj.GetType())) throw new InvalidCastException($"{nameof(obj)} is not the same type as {nameof(T)}. {nameof(obj)} must be the same type {nameof(T)}");
            var type = typeof(T);
            var definitionReader = typeof(SchemaDefinitionReader<>).MakeGenericType(type);
            var instance = Activator.CreateInstance(definitionReader, true);
            return (SchemaDefinitionReader<T>)instance;
        }

        /// <inheritdoc />
        public bool AttributeIsDefinedOnEntity<TAttribute>() where TAttribute : Attribute
        {
            return typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Any(e => e.GetCustomAttribute(typeof(TAttribute)) != null);
        }

        /// <inheritdoc />
        public string GetEntityNameByAttribute<TAttribute>(TEntity instance) where TAttribute : Attribute
        {
            PropertyInfo firstPropertyMatchingAttribute = typeof(TEntity)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(e => e.GetCustomAttribute(typeof(TAttribute)) != null);
            return firstPropertyMatchingAttribute?.Name;
        }

        /// <inheritdoc />
        public TAttribute GetAttributeInstanceFromType<TAttribute>() where TAttribute : Attribute
        {
            return (TAttribute)Attribute.GetCustomAttribute(typeof(TEntity), typeof(TAttribute));
        }

        /// <inheritdoc />
        public object ReadAttributeValue<TAttribute>(TEntity instance) where TAttribute : Attribute
        {
            PropertyInfo firstPropertyMatchingAttribute = typeof(TEntity)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(e => e.GetCustomAttribute(typeof(TAttribute)) != null);
            if (firstPropertyMatchingAttribute == null) return null;
            return firstPropertyMatchingAttribute.GetValue(instance);
        }
    }
}
