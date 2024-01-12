using Cosmos.Entity.Mapper.Internal;
using Cosmos.Entity.Mapper.Schema;
using Microsoft.Azure.Cosmos;
using Moq;
using NUnit.Framework;

namespace Cosmos.Entity.Mapper.Tests
{
    public class OfTypeBaseTestClass
    {
        [DocumentKey]
        public int Id { get; set; }

        [DocumentPartition]
        public int PartitionKey { get; set; }
    }

    public class OfTypeDerivedClass : OfTypeBaseTestClass { }

    [TestFixture()]
    public class CollectionSetTests
    {
        [Test()]
        public void OfType_Can_Project_To_Derived_Object()
        {
            var collectionSet = new InternalCollectionSet<OfTypeBaseTestClass>(new Mock<Container>().Object, new ContextOptions { }).OfType<OfTypeDerivedClass>();
            Assert.That(collectionSet, Is.Not.Null);
        }
    }
}