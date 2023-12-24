using Cosmos.Entity.Mapper.Extensions;
using Bogus;
using NUnit.Framework;
using System.Linq;
using System;

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

        public class DynamicPropertyTest
        {
            public int Prop1 { get; set; }
        }

        [Test()]
        public void OrderByDynamicProperty_Can_Throw_Exception_When_Property_Does_Not_Exist_In_Entity()
        {
            var stub = new Faker<DynamicPropertyTest>()
                .RuleFor(e => e.Prop1,e => e.UniqueIndex)
                .Generate(10)
                .AsQueryable();
            Assert.Throws<InvalidOperationException>(() => stub.OrderByDynamicProperty("NoneExistingProperty", System.ComponentModel.ListSortDirection.Ascending));
        }

        [Test()]
        public void OrderByDynamicProperty_Can_Order_Query_Using_Case_Insensitive_Property_Name()
        {
            var stub = new Faker<DynamicPropertyTest>()
                .RuleFor(e => e.Prop1, e => e.UniqueIndex)
                .Generate(10)
                .AsQueryable();
            var actual = stub.OrderByDynamicProperty("prop1", System.ComponentModel.ListSortDirection.Descending).FirstOrDefault();
            var compare = stub.OrderByDescending(e => e.Prop1).FirstOrDefault();
            Assert.That(actual, Is.EqualTo(compare));
        }

        [Test()]
        public void OrderByDynamicProperty_Can_Order_Query_After_Where_Clause_IsApplied()
        {
            var stub = new Faker<DynamicPropertyTest>()
                .RuleFor(e => e.Prop1, e => e.UniqueIndex)
                .Generate(10)
                .AsQueryable();
            var actual = stub.Where(e => e.Prop1 != 0).OrderByDynamicProperty("Prop1", System.ComponentModel.ListSortDirection.Descending).FirstOrDefault();
            var compare = stub.Where(e => e.Prop1 != 0).OrderByDescending(e => e.Prop1).FirstOrDefault();
            Assert.That(actual, Is.EqualTo(compare));
        }
    }
}