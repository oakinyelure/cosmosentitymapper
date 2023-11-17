namespace Cosmos.Entity.Mapper.Utilities
{
    using Cosmos.Entity.Mapper.Schema;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// This is an internal implementation and not designed to be publicly used. Changes can be made without a public notice
    /// Use is a production environment may lead to unpredictable issues
    /// </summary>
    public abstract class EntityValidationBase
    {
        /// <summary>
        /// Checks if an entity is null.
        /// </summary>
        /// <remarks>
        /// <para>Method explictly checks for null. A whitespace and empty string would not pass through as they are not null</para>
        /// </remarks>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static TEntity NotNullOrThrow<TEntity>(TEntity entity)
        {
            bool entityIsValid = entity is not null;
            if (!entityIsValid) throw new ArgumentNullException(entity?.GetType().Name ?? nameof(entity));
            return entity;
        }

        /// <summary>
        /// Checks if an entity evaluates to true
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <remarks>
        /// <para>Uses the <paramref name="entity"/> default <see cref="IConvertible.ToBoolean(IFormatProvider)"/> to check if an object evaluates to true</para>
        /// </remarks>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static TEntity MustBeTrueOrThrow<TEntity>(TEntity entity)
        {
            if(entity is not IConvertible) 
            { 
                throw new ArgumentException($"{nameof(entity)} cannot be converted to an IConvertible");
            }
            bool entityIsValid = false;
            try
            {
                entityIsValid = ((IConvertible)entity).ToBoolean(null);
            }
            catch (InvalidCastException) { }
            catch (FormatException) { }
            return entityIsValid ? entity : throw new ArgumentException($"{nameof(entity)} evaluates to false");
        }

        /// <summary>
        /// Evaluates <paramref name="entities"/> and throws and exception if param is null 
        /// of or is empty
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="entities"/> is null</exception>
        public static IEnumerable<TEntity> NotEmptyOrThrow<TEntity>(IEnumerable<TEntity> entities)
        {
            NotNullOrThrow(entities);
            return entities.Any() ? entities : throw new ArgumentException($"{nameof(entities)} cannot be empty");
        }

        /// <summary>
        /// Evaluates if an enumerable has a null entity. 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="entities"/> is null</exception>
        public static IEnumerable<TEntity> HasNoNullEntityOrThrow<TEntity>(IEnumerable<TEntity> entities)
        {
            NotNullOrThrow(entities);
            return entities.Any(entity => entity is null) ? throw new ArgumentException($"{nameof(entities)} cannot contain a null entity") : entities;
        }

        /// <summary>
        /// Evaluates that all documents in <paramref name="documents"/> has a value in the Id value else throws an exception
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="documents"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<TDocument> HasNoDocumentWithNullableIdOrThrow<TDocument>(IEnumerable<TDocument> documents)
            where TDocument : class
        {
            NotNullOrThrow(documents);
            var documentSchemaReader = new SchemaDefinitionReader<TDocument>();
            return documents.Any(document => documentSchemaReader.ReadAttributeValue<DocumentKeyAttribute>(document) is null) ? throw new ArgumentException($"{nameof(documents)} cannot contain a null entity") : documents;
        }
    }
}
