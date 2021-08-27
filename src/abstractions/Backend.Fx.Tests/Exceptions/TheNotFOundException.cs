using Backend.Fx.Exceptions;
using Backend.Fx.Tests.BuildingBlocks;
using Xunit;

namespace Backend.Fx.Tests.Exceptions
{
    public class TheNotFoundException
    {
        [Fact]
        public void FillsNameAndIdProperties()
        {
            var exception = new NotFoundException<TheAggregateRoot.TestAggregateRoot>(4711);
            Assert.Equal("TestAggregateRoot", exception.EntityName);
            Assert.Equal(4711, exception.Id);
        }
    }
}