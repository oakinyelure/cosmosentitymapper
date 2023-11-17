using System;

namespace Cosmos.Entity.Mapper.Schema
{
    /// <summary>
    /// Define database attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DatabaseAttribute : Attribute
    {       
        internal string DatabaseName { get; private set; }

        /// <summary>
        /// Configure the database with its name
        /// </summary>
        /// <param name="name"></param>
        public DatabaseAttribute(string name)
        {
            DatabaseName = name;
        }
    }
}
