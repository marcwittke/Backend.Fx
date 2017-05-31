namespace Backend.Fx.Tests.Exceptions
{
    using BuildingBlocks;
    using Fx.Exceptions;
    using NLogLogging;
    using Xunit;

    public class TheNotFoundException : IClassFixture<NLogLoggingFixture>
    {
        [Fact]
        public void FillsNameAndIdProperties()
        {
            var exception = new NotFoundException<TheAggregateRoot.TestAggregateRoot>(4711);
            Assert.Equal("TestAggregateRoot", exception.AggregateName);
            Assert.Equal(4711, exception.Id);
        }
    }
}
