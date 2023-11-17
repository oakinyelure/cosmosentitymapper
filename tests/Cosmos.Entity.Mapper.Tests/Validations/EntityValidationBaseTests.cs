using Bogus;
using Cosmos.Entity.Mapper.Schema;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cosmos.Entity.Mapper.Utilities.Tests
{
    public class EntityValidationBaseStubWithId
    {
        [DocumentKey]
        public int? Id { get; set; }

        public DateTimeOffset CreatedOn { get; set; }
    }

    [TestFixture()]
    public class EntityValidationBaseTests
    {

        [Test()]
        public void NotNullOrThrow_Throws_When_Entity_Is_Null()
        {
            object entity = null;
            Assert.Throws<ArgumentNullException>(() => EntityValidationBase.NotNullOrThrow(entity));
        }

        [Theory()]
        [TestCase(false)]
        [TestCase(0)]
        [TestCase("false")]
        [TestCase(null)]
        public void MustBeTrueOrThrow_Throws_Exception_When_Entity_Evaluates_To_False(object entity)
        {
            Assert.Throws<ArgumentException>(() => EntityValidationBase.MustBeTrueOrThrow(entity));
        }

        [Test()]
        public void NotEmptyOrThrow_Throws_Exception_When_Collection_Is_Empty()
        {
            Assert.Throws<ArgumentException>(() => EntityValidationBase.NotEmptyOrThrow(Enumerable.Empty<object>()));
        }

        [Test()]
        public void HasNoNullEntityOrThrow_Throws_Exception_When_Enumerable_Contains_Null()
        {
            IEnumerable<object> entities = new List<object> { null, "string" };
            Assert.Throws<ArgumentException>(() => EntityValidationBase.HasNoNullEntityOrThrow(entities));
        }

        [Test()]
        public void HasNoDocumentWithNullableId_Can_Check_If_Documents_In_An_Enumerable_All_Have_Id()
        {
            short expected = 841;
            var stub = new Faker<EntityValidationBaseStubWithId>()
                .RuleFor(e => e.Id, e => e.UniqueIndex)
                .RuleFor(e => e.CreatedOn, e => e.Date.FutureOffset())
                .Generate(expected);
            var actual = EntityValidationBase.HasNoDocumentWithNullableIdOrThrow(stub);
            Assert.That(actual.Count(), Is.EqualTo(expected));
        }

        [Test()]
        public void HasNoDocumentWithNullableIdOrThrow_Can_Throw_Exception_If_Argument_Contains_Document_With_A_Null_Id()
        {
            short expected = 841;
            var stub = new Faker<EntityValidationBaseStubWithId>()
                .RuleFor(e => e.Id, e => e.UniqueIndex)
                .RuleFor(e => e.CreatedOn, e => e.Date.FutureOffset())
                .Generate(expected);
            stub.ElementAtOrDefault(6).Id = null;
            Assert.Throws<ArgumentException>(() => EntityValidationBase.HasNoDocumentWithNullableIdOrThrow(stub));
        }
    }
}