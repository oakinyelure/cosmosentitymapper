namespace Cosmos.Entity.Mapper.Schema
{
    using System;

    /// <summary>
    /// Contracts for attributes on a document
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface ISchemaDefinitionReader<TEntity>
    {
        /// <summary>
        /// Reads the value of the first property within the <paramref name="instance"/> argument that matches
        /// <typeparamref name="TAttribute"/>
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        object ReadAttributeValue<TAttribute>(TEntity instance) where TAttribute : Attribute;

        /// <summary>
        /// Checks if <typeparamref name="TAttribute"/> is defined on <typeparamref name="TEntity"/>
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to check for</typeparam>
        /// <returns>True if the attribute is defined</returns>
        bool AttributeIsDefinedOnEntity<TAttribute>() where TAttribute : Attribute;

        /// <summary>
        /// Finds the first first occurence <typeparamref name="TAttribute"/> on an entity where an entity
        /// could be a type, a property or any struct and returns the name of the property
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="instance"></param>
        /// <returns>The string interpretation of the entity name</returns>
        string GetEntityNameByAttribute<TAttribute>(TEntity instance) where TAttribute : Attribute;

        /// <summary>
        /// Gets instance of an attribute defined on the entity
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        TAttribute GetAttributeInstanceFromType<TAttribute>() where TAttribute : Attribute;
    }
}
