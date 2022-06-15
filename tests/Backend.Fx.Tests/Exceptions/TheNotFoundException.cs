using Backend.Fx.Exceptions;
using Backend.Fx.Tests.BuildingBlocks;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Exceptions
{
    public class TheNotFoundException : TestWithLogging
    {
        [Fact]
        public void FillsNameAndIdProperties()
        {
            var exception = new NotFoundException<TheAggregateRoot.TestAggregateRoot>(4711);
            Assert.Equal("TestAggregateRoot", exception.EntityName);
            Assert.Equal(4711, exception.Id);
        }

        public TheNotFoundException(ITestOutputHelper output) : base(output)
        {
        }
    }
}