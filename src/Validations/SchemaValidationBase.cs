namespace Cosmos.Entity.Mapper.Validations
{
    using Cosmos.Entity.Mapper.Schema;
    using System;

    /// <summary>
    /// Base class for classes performing schema validations
    /// </summary>
    public abstract class SchemaValidationBase<TDocument>
        where TDocument : class
    {
        internal static readonly ISchemaDefinitionReader<TDocument> _schemaDefinition = new SchemaDefinitionReader<TDocument>();
        /// <summary>
        /// Checks if <typeparamref name="TDocument"/> has a specified <see cref="DocumentKeyAttribute"/> attribute
        /// </summary>
        /// <exception cref="ArgumentException">Throws when entity does not have a key configuration</exception>
        public static bool MustHaveKeyDefinitionOrThrow()
        {
            return _schemaDefinition.AttributeIsDefinedOnEntity<DocumentKeyAttribute>() ? true : throw new InvalidOperationException($"{nameof(TDocument)} does not have required {nameof(DocumentKeyAttribute)}");
        }

        /// <summary>
        /// Checks if <typeparamref name="TDocument"/> has a required <see cref="DocumentPartitionAttribute"/> attribute
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Throws when entity does not have a partition key schema definition</exception>
        public static bool MustHavePartitionDefinitionOrThrow()
        {
            return _schemaDefinition.AttributeIsDefinedOnEntity<DocumentPartitionAttribute>() ? true : throw new InvalidOperationException($"{nameof(TDocument)} does not have required {nameof(DocumentPartitionAttribute)}");
        }


        /// <exception cref="InvalidOperationException">Throws when entity does not have a partition key schema definition</exception>
        /// <exception cref="ArgumentException">Throws when entity does not have a key configuration</exception>
        /// <remarks>
        /// <para>Checks if <typeparamref name="TDocument"/> has a specified <see cref="DocumentKeyAttribute"/> attribute</para>
        /// <para>Checks if <typeparamref name="TDocument"/> has a required <see cref="DocumentPartitionAttribute"/> attribute</para>
        /// </remarks>
        public static bool MustHaveMinumumSchemaDefinitionOrThrow()
        {
            return MustHaveKeyDefinitionOrThrow() && MustHavePartitionDefinitionOrThrow();
        }
    }
}
