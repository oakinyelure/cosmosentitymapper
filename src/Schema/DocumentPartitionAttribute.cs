using System;

namespace Cosmos.Entity.Mapper.Schema
{
    /// <summary>
    /// <see cref="DocumentPartitionAttribute"/> describes the partition key of a document.    
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DocumentPartitionAttribute : Attribute
    {
        /// <summary>
        /// Parameterless attribute
        /// </summary>
        public DocumentPartitionAttribute()
        {
        }
    }
}
