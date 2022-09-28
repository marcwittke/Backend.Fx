using Backend.Fx.Domain;
using Backend.Fx.Exceptions;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Exceptions
{
    public class TheNotFoundException : TestWithLogging
    {
        public TheNotFoundException(ITestOutputHelper output) : base(output)
        { }

        [Fact]
        public void CanBeThrownWithoutAnyParameters()
        {
            var exception = new NotFoundException();
            Assert.Null(exception.EntityName);
        }
        
        [Fact]
        public void FillsNameAndIdProperties()
        {
            var exception = new NotFoundException<SomeEntity>(4711);
            Assert.Equal("SomeEntity", exception.EntityName);
            Assert.Equal(4711, exception.Id);
        }


        [UsedImplicitly]
        private class SomeEntity : IAggregateRoot<int>
        {
            public int Id { get; }
        }
    }
}