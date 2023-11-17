using Bogus;
using Cosmos.Entity.Mapper;
using Microsoft.Azure.Cosmos;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.Entity.Mapper.Tests
{
    public class CollectionQueryableStub
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [TestFixture()]
    public class QueryMaterializerTests
    {
        private Mock<FeedIterator<CollectionQueryableStub>> _feedIteratorMock;
        private Mock<FeedResponse<CollectionQueryableStub>> _feedResponseMock;


        [Test()]
        public async Task ToEnumerableAsync_Can_Return_An_Enumerable_Of_Entity_When_Queried_Against_A_Queryable()
        {
            var stubs = new List<CollectionQueryableStub>
            {
                new CollectionQueryableStub { Id = 1, Name = "1" },
                new CollectionQueryableStub { Id = 2, Name = "2" },
                new CollectionQueryableStub { Id = 3, Name = "3" },
                new CollectionQueryableStub { Id = 4, Name = "4" },
            }.AsEnumerable();

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();
            ConfigureFeedIterator(stubs);
            queryExecutor.Setup(mock => mock.ExecuteAsync(It.IsAny<Task<FeedResponse<CollectionQueryableStub>>>())).ReturnsAsync(_feedResponseMock.Object);

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.ToFeedIterator()).Returns(_feedIteratorMock.Object);
            var actual = await collectionQueryableMock.Object.AsEnumerableAsync();
            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual.Count(), Is.EqualTo(stubs.Count()));
        }


        [Test()]
        public async Task FirstOrDefaultAsync_Returns_First_Element_In_A_Sequence()
        {
            var stubs = new List<CollectionQueryableStub>
            {
                new CollectionQueryableStub { Id = 1, Name = "1" },
                new CollectionQueryableStub { Id = 2, Name = "2" },
                new CollectionQueryableStub { Id = 3, Name = "3" },
                new CollectionQueryableStub { Id = 4, Name = "4" },
            }.AsEnumerable();

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.AsEnumerableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stubs);
            var actual = await collectionQueryableMock.Object.FirstOrDefaultAsync();
            Assert.That(actual, Is.Not.Null);
        }

        [Test()]
        public async Task FirstOrDefaultAsync_Returns_Null_When_No_Item_Is_In_The_Sequence()
        {
            var stubs = Enumerable.Empty<CollectionQueryableStub>();

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.AsEnumerableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stubs);
            var actual = await collectionQueryableMock.Object.FirstOrDefaultAsync();
            Assert.That(actual, Is.Null);
        }


        [Test()]
        public async Task SingleOrDefaultAsync_Can_Return_Single_Entity_From_Queryable_When_Sequence_Contains_Only_One_Document()
        {
            var stubs = new Faker<CollectionQueryableStub>()
                .RuleFor(e => e.Id, e => e.UniqueIndex)
                .Generate(1);

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.AsEnumerableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stubs);
            var actual = await collectionQueryableMock.Object.SingleOrDefaultAsync();
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(stubs.ElementAt(0)));
        }


        [Test()]
        public async Task SingleOrDefaultAsync_Can_Return_Null_From_Queryable_When_Sequence_Contains_No_Documents()
        {
            var stubs = Enumerable.Empty<CollectionQueryableStub>();

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.AsEnumerableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stubs);
            var actual = await collectionQueryableMock.Object.SingleOrDefaultAsync();
            Assert.That(actual, Is.Null);
        }

        [Test()]
        public void SingleOrDefaultAsync_Can_Throw_Exception_When_There_Are_More_Than_One_Document_In_The_Sequence()
        {
            var stubs = new Faker<CollectionQueryableStub>()
                .RuleFor(e => e.Id, e => e.UniqueIndex)
                .GenerateBetween(1, 7);

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.AsEnumerableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stubs);
            Assert.ThrowsAsync<InvalidOperationException>(() => collectionQueryableMock.Object.SingleOrDefaultAsync());
        }

        [Test()]
        public async Task FirstAsync_Can_Return_A_Single_Document_From_A_Sequence()
        {
            var stubs = new Faker<CollectionQueryableStub>()
                .RuleFor(e => e.Id, e => e.UniqueIndex)
                .GenerateBetween(1, 7);

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.AsEnumerableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stubs);
            var actual = await collectionQueryableMock.Object.FirstAsync();
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(stubs.ElementAt(0)));
        }

        [Test()]
        public void FirstAsync_Can_Throw_Exception_When_Sequence_Is_Empty()
        {
            var stubs = new Faker<CollectionQueryableStub>()
                .RuleFor(e => e.Id, e => e.UniqueIndex)
                .Generate(0);

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.AsEnumerableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stubs);
            Assert.ThrowsAsync<InvalidOperationException>(() => collectionQueryableMock.Object.FirstAsync());
        }

        [Test()]
        public async Task SingleAsync_Can_Return_The_Only_Document_In_Sequence()
        {
            var stubs = new Faker<CollectionQueryableStub>()
                .RuleFor(e => e.Id, e => e.UniqueIndex)
                .Generate(1);

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.AsEnumerableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stubs);
            var actual = await collectionQueryableMock.Object.SingleAsync();
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(stubs.ElementAt(0)));
        }

        [Test()]
        public void SingleAsync_Can_Throw_Exception_When_The_Sequence_Is_Empty()
        {
            var stubs = new Faker<CollectionQueryableStub>()
                .RuleFor(e => e.Id, e => e.UniqueIndex)
                .Generate(0);

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.AsEnumerableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stubs);
            Assert.ThrowsAsync<InvalidOperationException>(() => collectionQueryableMock.Object.SingleAsync());
        }

        [Test()]
        public void SingleAsync_Can_Throw_Exception_When_There_Are_More_Than_One_Document_In_The_Sequence()
        {
            var stubs = new Faker<CollectionQueryableStub>()
                .RuleFor(e => e.Id, e => e.UniqueIndex)
                .GenerateBetween(2, 8);

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.AsEnumerableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stubs);
            Assert.ThrowsAsync<InvalidOperationException>(() => collectionQueryableMock.Object.SingleAsync());
        }

        [Test(), Description("Would try connecting to the account endpoint but it is expected to not work. A little network latency but it's alright")]
        public void ToFeedIterator_Can_Return_Queryable_Cast_As_A_Feed_Iterator()
        {
            string connectionString = "AccountEndpoint=https://localhost;AccountKey=VGhpc2lzc3VwcG9zZWR0b2JlYWJhc2U2NHN0cmluZw==;Database=testDatabase";
            var cosmosClient = new CosmosClient(connectionString);
            var db = cosmosClient.GetDatabase("TestDb");
            var container = db.GetContainer("TestContainer");
            var stubs = container.GetItemLinqQueryable<CollectionQueryableStub>();

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryMaterializer = new QueryMaterializer<CollectionQueryableStub>(stubs, queryExecutor.Object);
            var actual = queryMaterializer.ToFeedIterator();
            Assert.That(actual, Is.InstanceOf<FeedIterator<CollectionQueryableStub>>());
        }

        [Test()]
        public async Task ToListAsync_Can_Return_A_List_From_The_Materialized_Sequence()
        {
            var stubs = new Faker<CollectionQueryableStub>()
                .RuleFor(e => e.Id, e => e.UniqueIndex)
                .GenerateBetween(1, 7);

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.AsEnumerableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stubs);
            var actual = await collectionQueryableMock.Object.ToListAsync();
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual, Is.InstanceOf<List<CollectionQueryableStub>>());
            Assert.That(actual.Count(), Is.EqualTo(stubs.Count()));
        }

        [Test()]
        public async Task AnyAsync_Can_Return_True_When_Documents_Are_In_The_Sequence()
        {
            var stubs = new Faker<CollectionQueryableStub>()
                .RuleFor(e => e.Id, e => e.UniqueIndex)
                .GenerateBetween(1, 7);

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.AsEnumerableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stubs);
            var actual = await collectionQueryableMock.Object.AnyAsync();
            Assert.True(actual);
        }

        [Test()]
        public async Task AnyAsync_Can_Return_False_When_No_Documents_Are_In_The_Sequence()
        {
            var stubs = new Faker<CollectionQueryableStub>()
                .RuleFor(e => e.Id, e => e.UniqueIndex)
                .Generate(0);

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.AsEnumerableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stubs);
            var actual = await collectionQueryableMock.Object.AnyAsync();
            Assert.False(actual);
        }


        [Test()]
        public async Task AnyAsync_Can_Return_True_When_Documents_Are_In_The_Sequence_After_Checking_The_Predicate()
        {
            var stubs = new Faker<CollectionQueryableStub>()
                .RuleFor(e => e.Id, e => e.UniqueIndex)
                .RuleFor(e => e.Name, e => e.Person.FullName)
                .GenerateBetween(1, 7);

            var queryExecutor = new Mock<IQueryExecutor<CollectionQueryableStub>>();
            var queryableMock = new Mock<IQueryable<CollectionQueryableStub>>();

            var collectionQueryableMock = new Mock<QueryMaterializer<CollectionQueryableStub>>(MockBehavior.Loose, stubs.AsQueryable(), queryExecutor.Object) { CallBase = true };
            collectionQueryableMock.Setup(mock => mock.AsEnumerableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stubs);
            var actual = await collectionQueryableMock.Object.AnyAsync(e => e.Name != null);
            Assert.True(actual);
        }

        private FeedIterator<CollectionQueryableStub> ConfigureFeedIterator(IEnumerable<CollectionQueryableStub> entities)
        {
            _feedIteratorMock = new Mock<FeedIterator<CollectionQueryableStub>>();
            _feedIteratorMock.SetupSequence(iterator => iterator.HasMoreResults)
                .Returns(true)
                .Returns(true)
                .Returns(false);
            _feedResponseMock = new Mock<FeedResponse<CollectionQueryableStub>>();
            _feedResponseMock.SetupGet(e => e.Resource).Returns(entities);
            _feedResponseMock.Setup(e => e.GetEnumerator()).Returns(entities.GetEnumerator());
            _feedIteratorMock.Setup(iterator => iterator.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _feedResponseMock.Object);
            return _feedIteratorMock.Object;
        }


    }
}