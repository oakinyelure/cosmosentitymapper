using Cosmos.Entity.Mapper.Schema;
using NUnit.Framework;
using System;

namespace Cosmos.Entity.Mapper.Validations.Tests
{
    public class SchemaValidationBaseTestsStubHasSchema
    {
        [DocumentKey]
        public string Id { get; set; }

        [DocumentPartition]
        public string Partition { get; set; }
    }

    public class SchemaValidationBaseTestsStubNoSchema
    {
        public string Id { get; set; }

        public string Partition { get; set; }
    }

    [TestFixture()]
    public class SchemaValidationBaseTests
    {
        [Test()]
        public void MustHaveKeyDefinitionOrThrow_Returns_True_If_Document_Has_Key_Attribute()
        {
            Assert.That(SchemaValidationBase<SchemaValidationBaseTestsStubHasSchema>.MustHaveKeyDefinitionOrThrow(), Is.True);
        }

        [Test()]
        public void MustHaveKeyDefinitionOrThrow_Throws_Exception_If_Document_Has_No_Key_Attribute()
        {
            Assert.Throws<InvalidOperationException>(() => SchemaValidationBase<SchemaValidationBaseTestsStubNoSchema>.MustHaveKeyDefinitionOrThrow());
        }

        [Test()]
        public void MustHavePartitionDefinitionOrThrow_Returns_True_If_Document_Has_Partition_Attribute()
        {
            Assert.That(SchemaValidationBase<SchemaValidationBaseTestsStubHasSchema>.MustHavePartitionDefinitionOrThrow(), Is.True);
        }


        [Test()]
        public void MustHavePartitionDefinitionOrThrow_Throws_Exception_If_Document_Has_No_Partition_Attribute()
        {
            Assert.Throws<InvalidOperationException>(() => SchemaValidationBase<SchemaValidationBaseTestsStubNoSchema>.MustHavePartitionDefinitionOrThrow());
        }

    }
}