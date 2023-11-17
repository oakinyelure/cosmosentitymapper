namespace Cosmos.Entity.Mapper.Schema
{
    using System;
    /// <summary>
    /// The <see cref="DocumentKeyAttribute"/> is used to denote the id column of a document or entity
    /// This will make the document a unique entity within its partition
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DocumentKeyAttribute : Attribute
    {
        /// <summary>
        /// Parameterless key
        /// </summary>
        public DocumentKeyAttribute()
        {
        }
    }
}
