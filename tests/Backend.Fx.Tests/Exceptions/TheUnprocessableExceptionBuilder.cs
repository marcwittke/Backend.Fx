using Backend.Fx.Exceptions;
using Backend.Fx.Tests.BuildingBlocks;
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
            sut.AddNotFoundWhenNull<TheAggregateRoot.TestAggregateRoot>(1111, null);
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void AddsNoExceptionWhenAggregateIsNotNull()
        {
            IExceptionBuilder sut = UnprocessableException.UseBuilder();
            sut.AddNotFoundWhenNull(1111, new TheAggregateRoot.TestAggregateRoot(12345, "gaga"));
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
        public void ThrowsExceptionWhenAddingError()
        {
            IExceptionBuilder sut = UnprocessableException.UseBuilder();
            sut.Add("something is broken");
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        public TheUnprocessableExceptionBuilder(ITestOutputHelper output) : base(output)
        {
        }
    }
}