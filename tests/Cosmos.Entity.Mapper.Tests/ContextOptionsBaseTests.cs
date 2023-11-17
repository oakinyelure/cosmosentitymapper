using Moq;
using NUnit.Framework;
using System;
using System.Data.Common;

namespace Cosmos.Entity.Mapper.Tests
{
    public class ContextOptionsTestContext : CosmosDbContext
    {
        public ContextOptionsTestContext(string conn): base(conn) { }
    }

    [TestFixture()]
    public class ContextOptionsBaseTests
    {
        [Theory()]
        [TestCase("AccountEndpoint","","AccountKey")]
        [TestCase("AccountEndpoint","Database","")]
        [TestCase("AccountEndpoint","","")]
        [TestCase("","Database","AccountKey")]
        [TestCase("","","AccountKey")]
        public void EnforceMinimumConnectionStringRequirementAndThrow_Throws_Exception_When_Basic_Information_Is_Missing(string accountEndpoint, string accountKey, string database)
        {            
            var option = new ContextOptions { ConnectionString = string.Format("AccountEndPoint={0};AccountKey={1};Database={2}",accountEndpoint,accountKey,database)};
            var mockContext = new Mock<CosmosDbContext>(MockBehavior.Loose, "AccountEndpoint=https://localhost;AccountKey=VGhpc2lzc3VwcG9zZWR0b2JlYWJhc2U2NHN0cmluZw==;Database=testDatabase");
            var exception = Assert.Throws<ArgumentException>(() => option.EnforceMinimumConnectionStringRequirementAndThrow(new DbConnectionStringBuilder { ConnectionString = option.ConnectionString }, mockContext.Object));
        }

        [Test]
        public void EnforceMinimumConnectionStringRequirementAndThrow_Does_Not_Throw_Exception_When_Basic_Information_Is_Not_Missing()
        {
            var option = new ContextOptions { ConnectionString = "AccountEndpoint=https://localhost;AccountKey=VGhpc2lzc3VwcG9zZWR0b2JlYWJhc2U2NHN0cmluZw==;Database=testDatabase" };
            var mockContext = new ContextOptionsTestContext("AccountEndpoint=https://localhost;AccountKey=VGhpc2lzc3VwcG9zZWR0b2JlYWJhc2U2NHN0cmluZw==;Database=testDatabase");
            Assert.DoesNotThrow(() => option.EnforceMinimumConnectionStringRequirementAndThrow(new DbConnectionStringBuilder { ConnectionString = option.ConnectionString }, mockContext));
        }
    }
}