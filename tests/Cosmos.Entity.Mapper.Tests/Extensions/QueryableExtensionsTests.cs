using Bogus;
using NUnit.Framework;
using System.Linq;

namespace Cosmos.Entity.Mapper.Extensions.Tests
{
    [TestFixture()]
    public class QueryableExtensionsTests
    {
        [Test()]
        public void PaginateByOffset_Can_Paginate_Result_From_An_OrderedQueryable()
        {
            var stub = new Faker().Random.WordsArray(100).AsQueryable();
            var firstTen = stub.OrderByDescending(e => e).Take(10);
            var actual = stub.OrderByDescending(e => e).PaginateByOffset(0, 10).ToList();
            Assert.That(firstTen, Is.EquivalentTo(actual));
        }

        [Test()]
        public void PaginateByOffset_Can_From_A_Queryable()
        {
            var stub = new Faker().Random.WordsArray(100).AsQueryable();
            var firstTen = stub.Take(10);
            var actual = stub.PaginateByOffset(0, 10).ToList();
            Assert.That(firstTen, Is.EquivalentTo(actual));
        }
    }
}