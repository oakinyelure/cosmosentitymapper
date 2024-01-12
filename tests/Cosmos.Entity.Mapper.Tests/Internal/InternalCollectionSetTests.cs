using Bogus;
using Cosmos.Entity.Mapper.Schema;
using Microsoft.Azure.Cosmos;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.Entity.Mapper.Internal.Tests
{
    public class TestDocumentStub 
    {
        [DocumentKey]
        public string Id { get; set; }

        [DocumentPartition]
        public string PartitionKey { get; set; }
    }

    [TestFixture()]
    public class InternalCollectionSetTests
    {
        [Test()]
        public async Task FindAsync_Can_Find_Document_From_Collection()
        {
            var itemResponse = new Mock<ItemResponse<TestDocumentStub>>(MockBehavior.Loose) { CallBase = false };
            itemResponse.SetupGet(e => e.Resource).Returns(new TestDocumentStub { });
            var container = new Mock<Container>(MockBehavior.Strict) { CallBase = false };
            container.Setup(e => e.GetItemLinqQueryable<TestDocumentStub>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>(), It.IsAny<CosmosLinqSerializerOptions>()))
                .Returns(Enumerable.Empty<TestDocumentStub>().AsQueryable().OrderByDescending(e => e.Id));

            container.Setup(e => e.ReadItemAsync<TestDocumentStub>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(itemResponse.Object);
            container.SetupGet(e => e.Id).Returns("containerId");

            var collectionSet = new InternalCollectionSet<TestDocumentStub>(container.Object, new ContextOptions { });
            var actual = await collectionSet.FindAsync("id", "partitionKey", CancellationToken.None);
            Assert.That(actual, Is.Not.Null);
        }

        [Test()]
        public void FindAsync_ReThrows_Exception_When_Exception_Occurs_In_Execution()
        {
            var itemResponse = new Mock<ItemResponse<TestDocumentStub>>(MockBehavior.Loose) { CallBase = false };
            itemResponse.SetupGet(e => e.Resource).Returns(new TestDocumentStub { });
            var container = new Mock<Container>(MockBehavior.Strict) { CallBase = false };
            container.Setup(e => e.GetItemLinqQueryable<TestDocumentStub>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>(), It.IsAny<CosmosLinqSerializerOptions>()))
                .Returns(Enumerable.Empty<TestDocumentStub>().AsQueryable().OrderByDescending(e => e.Id));

            container.Setup(e => e.ReadItemAsync<TestDocumentStub>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CosmosException("Message", HttpStatusCode.InternalServerError, 500, "activityId", 6d));
            container.SetupGet(e => e.Id).Returns("containerId");

            var collectionSet = new InternalCollectionSet<TestDocumentStub>(container.Object, new ContextOptions { });
            Assert.ThrowsAsync<CosmosException>(() => collectionSet.FindAsync("id", "partitionKey", CancellationToken.None));
        }

        [Test()]
        public async Task AddAsync_Can_Add_Document_To_The_Container()
        {
            var expected = new TestDocumentStub();
            var itemResponse = new Mock<ItemResponse<TestDocumentStub>>(MockBehavior.Loose) { CallBase = false };
            itemResponse.SetupGet(e => e.Resource).Returns(expected);
            var container = new Mock<Container>(MockBehavior.Strict) { CallBase = false };
            container.Setup(e => e.GetItemLinqQueryable<TestDocumentStub>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>(), It.IsAny<CosmosLinqSerializerOptions>()))
                .Returns(Enumerable.Empty<TestDocumentStub>().AsQueryable().OrderByDescending(e => e.Id));
            container.Setup(e => e.CreateItemAsync(It.IsAny<TestDocumentStub>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(itemResponse.Object);
            container.SetupGet(e => e.Id).Returns("containerId");

            var collectionSet = new InternalCollectionSet<TestDocumentStub>(container.Object, new ContextOptions { });
            var actual = await collectionSet.AddAsync(new TestDocumentStub { });
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test()]
        public async Task AddRangeAsync_Can_Add_Many_Documents_To_The_Container_Efficiently()
        {
            var expected = new TestDocumentStub();
            var itemResponse = new Mock<ItemResponse<TestDocumentStub>>(MockBehavior.Loose) { CallBase = false };
            itemResponse.SetupGet(e => e.Resource).Returns(expected);
            var container = new Mock<Container>(MockBehavior.Strict) { CallBase = false };
            container.Setup(e => e.GetItemLinqQueryable<TestDocumentStub>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>(), It.IsAny<CosmosLinqSerializerOptions>()))
                .Returns(Enumerable.Empty<TestDocumentStub>().AsQueryable().OrderByDescending(e => e.Id));
            container.Setup(e => e.CreateItemAsync(It.IsAny<TestDocumentStub>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(itemResponse.Object);
            container.SetupGet(e => e.Id).Returns("containerId");

            var collectionSet = new InternalCollectionSet<TestDocumentStub>(container.Object, new ContextOptions { });
            var documents = new Faker<TestDocumentStub>()
                .RuleFor(e => e.Id, e => e.Random.Guid().ToString())
                .RuleFor(e => e.PartitionKey, e => e.Random.String())
                .GenerateBetween(1, 1000);
            int actual = await collectionSet.AddRangeAsync(documents);
            container.Verify(e => e.CreateItemAsync(It.IsAny<TestDocumentStub>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(documents.Count));
            Assert.That(actual, Is.EqualTo(documents.Count));
        }

        [Test()]
        public async Task UpdateAsync_Can_Update_Document_In_A_Container()
        {
            var expected = new TestDocumentStub();
            var itemResponse = new Mock<ItemResponse<TestDocumentStub>>(MockBehavior.Loose) { CallBase = false };
            itemResponse.SetupGet(e => e.Resource).Returns(expected);
            var container = new Mock<Container>(MockBehavior.Strict) { CallBase = false };
            container.Setup(e => e.GetItemLinqQueryable<TestDocumentStub>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>(), It.IsAny<CosmosLinqSerializerOptions>()))
                .Returns(Enumerable.Empty<TestDocumentStub>().AsQueryable().OrderByDescending(e => e.Id));
            container.Setup(e => e.UpsertItemAsync(It.IsAny<TestDocumentStub>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(itemResponse.Object);
            container.SetupGet(e => e.Id).Returns("containerId");

            var collectionSet = new InternalCollectionSet<TestDocumentStub>(container.Object, new ContextOptions { });
            var actual = await collectionSet.UpdateAsync(new TestDocumentStub { });
            container.Verify(e => e.UpsertItemAsync(It.IsAny<TestDocumentStub>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test()]
        public async Task UpdateRangeAsync_Can_Update_Many_Documents_In_A_Container()
        {
            var expected = new TestDocumentStub();
            var itemResponse = new Mock<ItemResponse<TestDocumentStub>>(MockBehavior.Loose) { CallBase = false };
            itemResponse.SetupGet(e => e.Resource).Returns(expected);
            var container = new Mock<Container>(MockBehavior.Strict) { CallBase = false };
            container.Setup(e => e.GetItemLinqQueryable<TestDocumentStub>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>(), It.IsAny<CosmosLinqSerializerOptions>()))
                .Returns(Enumerable.Empty<TestDocumentStub>().AsQueryable().OrderByDescending(e => e.Id));
            container.Setup(e => e.UpsertItemAsync(It.IsAny<TestDocumentStub>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(itemResponse.Object);
            container.SetupGet(e => e.Id).Returns("containerId");

            var collectionSet = new InternalCollectionSet<TestDocumentStub>(container.Object, new ContextOptions { });
            var documents = new Faker<TestDocumentStub>()
                .RuleFor(e => e.Id, e => e.Random.Guid().ToString())
                .RuleFor(e => e.PartitionKey, e => e.Random.String())
                .GenerateBetween(1, 1000);
            int actual = await collectionSet.UpdateRangeAsync(documents);
            container.Verify(e => e.UpsertItemAsync(It.IsAny<TestDocumentStub>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(documents.Count));
            Assert.That(actual, Is.EqualTo(documents.Count));
        }

        [Test()]
        public async Task RemoveAsync_Can_Delete_Item_From_Container()
        {
            var expected = new TestDocumentStub();
            var itemResponse = new Mock<ItemResponse<TestDocumentStub>>(MockBehavior.Loose) { CallBase = false };
            itemResponse.SetupGet(e => e.Resource).Returns(expected);
            var container = new Mock<Container>(MockBehavior.Strict) { CallBase = false };
            container.Setup(e => e.GetItemLinqQueryable<TestDocumentStub>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>(), It.IsAny<CosmosLinqSerializerOptions>()))
                .Returns(Enumerable.Empty<TestDocumentStub>().AsQueryable().OrderByDescending(e => e.Id));
            container.Setup(e => e.DeleteItemAsync<TestDocumentStub>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(itemResponse.Object);
            container.SetupGet(e => e.Id).Returns("containerId");

            var collectionSet = new InternalCollectionSet<TestDocumentStub>(container.Object, new ContextOptions { });
            var actual = await collectionSet.RemoveAsync(new TestDocumentStub { Id = Guid.NewGuid().ToString() });
            container.Verify(e => e.DeleteItemAsync<TestDocumentStub>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test()]
        public async Task RemoveRangeAsync_Can_Delete_Many_Items_From_Container()
        {
            var expected = new TestDocumentStub();
            var itemResponse = new Mock<ItemResponse<TestDocumentStub>>(MockBehavior.Loose) { CallBase = false };
            itemResponse.SetupGet(e => e.Resource).Returns(expected);
            var container = new Mock<Container>(MockBehavior.Strict) { CallBase = false };
            container.Setup(e => e.GetItemLinqQueryable<TestDocumentStub>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>(), It.IsAny<CosmosLinqSerializerOptions>()))
                .Returns(Enumerable.Empty<TestDocumentStub>().AsQueryable().OrderByDescending(e => e.Id));
            container.Setup(e => e.DeleteItemAsync<TestDocumentStub>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(itemResponse.Object);
            container.SetupGet(e => e.Id).Returns("containerId");

            var collectionSet = new InternalCollectionSet<TestDocumentStub>(container.Object, new ContextOptions { });
            var documents = new Faker<TestDocumentStub>()
                .RuleFor(e => e.Id, e => e.Random.Guid().ToString())
                .RuleFor(e => e.PartitionKey, e => e.Random.String())
                .GenerateBetween(1, 1000);
            int actual = await collectionSet.RemoveRangeAsync(documents);
            container.Verify(e => e.DeleteItemAsync<TestDocumentStub>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(documents.Count));
            Assert.That(actual, Is.EqualTo(documents.Count));
        }


    }
}