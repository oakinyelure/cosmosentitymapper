using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Cosmos.Entity.Mapper.Diagnostics.Logging.Tests
{
    [TestFixture()]
    public class DiagnosticsLoggerTests
    {
        [Test()]
        public void Can_Create_Instance_Of_Logger_Factory_If_Non_Is_Assigned()
        {
            ILoggerFactory factory = DiagnosticsLogger.LoggerFactory;
            Assert.That(factory, Is.Not.Null);
        }
    }
}