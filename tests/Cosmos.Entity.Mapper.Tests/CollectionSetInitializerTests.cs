using Cosmos.Entity.Mapper.Schema;
using Microsoft.Azure.Cosmos;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace Cosmos.Entity.Mapper.Tests
{
    [Collection("TestCollections")]
    public partial class TestContextCollection
    {
        [DocumentKey]
        public int Id { get; set; }

        [DocumentPartition]
        public string PartitionKey { get; set; }
    }

    public class TestContext : CosmosDbContext
    {
        public TestContext(string connection):base(connection)
        {            
        }

        public CollectionSet<TestContextCollection> ObjectContextCollections { get; set; }

        public CollectionSet<TestContextCollection> StringCollections { get; set; }
        public CollectionSet<TestContextCollection> TestContextCollections { get; set; }
    }

    [TestFixture()]
    public class CollectionSetInitializerTests
    {
        private readonly string _connectionString = "AccountEndpoint=https://testendpoint.com;AccountKey=VGhpc2lzc3VwcG9zZWR0b2JlYWJhc2U2NHN0cmluZw==;Database=testDatabase";
        [Test()]
        public void GetSetsFromContext_Returns_All_Collection_Set_In_A_Context()
        {
            var collectionInitializer = new CollectionSetInitializer();
            var matchingProperties = collectionInitializer.GetCollectionSetFromContext(new TestContext(_connectionString));
            Assert.That(matchingProperties.Count(), Is.EqualTo(3));
        }

        [Test()]
        public void Can_Initialize_Collection_Set_From_The_Db_Context()
        {
            var collectionInitializer = new CollectionSetInitializer();
            var context = new TestContext(_connectionString);
            var databaseMock = new Mock<Database>(MockBehavior.Loose);
            var containerMock = new Mock<Container>(MockBehavior.Loose);
            databaseMock.Setup(e => e.GetContainer(It.IsAny<string>())).Returns(containerMock.Object);
            containerMock.SetupGet(e => e.Id).Returns("TestCollections");
            collectionInitializer.InitializeCollectionSet(context,databaseMock.Object, new ContextOptions { });
            Assert.That(context.TestContextCollections, Is.Not.Null);
            Assert.That(context.TestContextCollections.CollectionId, Is.EqualTo("TestCollections"));
        }
    }
}