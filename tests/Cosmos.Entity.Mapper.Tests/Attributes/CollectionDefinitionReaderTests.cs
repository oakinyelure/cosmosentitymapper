using Cosmos.Entity.Mapper.Schema;
using NUnit.Framework;
using System;

namespace Cosmos.Entity.Mapper.Attributes.Tests
{
    public class SomeEntity
    {
        [DocumentKey]
        public string SomeKey { get => "SomeKeyValue"; }

    }

    [Collection("SomeEntityWithType")]
    public class SomeEntityWithType
    {
        [DocumentPartition]
        public DateTimeOffset? SomeDate { get => DateTimeOffset.UtcNow; }
    }

    [TestFixture()]
    public class CollectionDefinitionReaderTests
    {
        [Test()]
        public void Can_Check_If_Attribute_Is_Defined_On_A_Collection()
        {
            var definitionReader = new SchemaDefinitionReader<SomeEntityWithType>();
            bool actual = definitionReader.AttributeIsDefinedOnEntity<DocumentPartitionAttribute>();
            Assert.That(actual, Is.True);
        }

        [Test()]
        public void Returns_The_Name_Of_Property_That_Has_Supported_Attribute()
        {
            var reader = new SchemaDefinitionReader<SomeEntity>();
            var actual = reader.GetEntityNameByAttribute<DocumentKeyAttribute>(new SomeEntity());
            Assert.That(actual, Is.EqualTo("SomeKey"));
        }

        [Test()]
        public void Returns_Instance_Of_Attribute_On_An_Entity_When_Present()
        {
            var reader = new SchemaDefinitionReader<SomeEntityWithType>();
            CollectionAttribute actual = reader.GetAttributeInstanceFromType<CollectionAttribute>();
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Name, Is.EqualTo("SomeEntityWithType"));
        }

        [Test()]
        public void Reads_The_Value_Of_First_Property_That_Has_Attribute()
        {
            var definitionReader = new SchemaDefinitionReader<SomeEntityWithType>();
            var instance = new SomeEntityWithType();
            var actual = definitionReader.ReadAttributeValue<DocumentPartitionAttribute>(instance);
            Assert.That(actual, Is.InstanceOf<DateTimeOffset>());
        }

        [Test()]
        public void Can_Create_Instance_Of_Collection_Definition_From_Type()
        {
            var desiredType = typeof(SomeEntityWithType);
            var reader = SchemaDefinitionReader<object>.GetInstanceFromType(desiredType);
            Assert.That(reader, Is.TypeOf<SchemaDefinitionReader<SomeEntityWithType>>());
        }
    }
}