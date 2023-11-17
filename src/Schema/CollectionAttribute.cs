using System;

namespace Cosmos.Entity.Mapper.Schema
{
    /// <summary>
    /// Configurable attributes to enable the mapping of an entity 
    /// to a specific collection
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class CollectionAttribute : Attribute
    {
        /// <summary>
        ///  The name of the collection
        /// </summary>
        /// <remarks>
        /// <para>Cosmos Db refers to this as a container Id</para>
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Sets the target collection name
        /// </summary>
        /// <param name="collectionName"></param>
        public CollectionAttribute(string collectionName)
        {
            Name = collectionName;
        }
    }
}
