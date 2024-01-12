using NUnit.Framework;
using System.Linq;

namespace Cosmos.Entity.Mapper.Tests
{
    [TestFixture()]
    public class EntityMapperExtensionsTests
    {
        [Test()]
        public void ToMaterializable_Can_Convert_IQueryable_To_IQueryMaterializable()
        {
            var stub = Enumerable.Empty<string>().AsQueryable();
            var actual = stub.AsMaterializable();
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual is IQueryMaterializable<string>, Is.True);
        }
    }
}