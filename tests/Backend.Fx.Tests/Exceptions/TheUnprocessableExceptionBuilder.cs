using Backend.Fx.Domain;
using Backend.Fx.Exceptions;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Exceptions
{
    public class TheUnprocessableExceptionBuilder : TestWithLogging
    {
        [Fact]
        public void AddsExceptionWhenAggregateIsNull()
        {
            IExceptionBuilder sut = UnprocessableException.UseBuilder();
            sut.AddNotFoundWhenNull<SomeEntity>(1111, null);
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void AddsNoExceptionWhenAggregateIsNotNull()
        {
            IExceptionBuilder sut = UnprocessableException.UseBuilder();
            sut.AddNotFoundWhenNull(1111, new SomeEntity());
            sut.Dispose();
        }

        [Fact]
        public void DoesNotThrowExceptionWhenNotAddingConditionalError()
        {
            IExceptionBuilder sut = UnprocessableException.UseBuilder();
            sut.AddIf(false, "something is broken");
            sut.Dispose();
        }

        [Fact]
        public void ThrowsExceptionWhenAddingConditionalError()
        {
            IExceptionBuilder sut = UnprocessableException.UseBuilder();
            sut.AddIf(true, "something is broken");
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }
        
        [Fact]
        public void ThrowsExceptionWhenAddingConditionalKeyedError()
        {
            IExceptionBuilder sut = UnprocessableException.UseBuilder();
            sut.AddIf("the key", true, "something is broken");
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void ThrowsExceptionWhenAddingError()
        {
            IExceptionBuilder sut = UnprocessableException.UseBuilder();
            sut.Add("something is broken");
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }
        
        [Fact]
        public void ThrowsExceptionWhenAddingKeyedError()
        {
            IExceptionBuilder sut = UnprocessableException.UseBuilder();
            sut.Add("theKey", "something is broken");
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        public TheUnprocessableExceptionBuilder(ITestOutputHelper output) : base(output)
        {
        }
        
        [UsedImplicitly]
        private class SomeEntity : IAggregateRoot<int>
        {
            public int Id { get; }
        }
    }
}