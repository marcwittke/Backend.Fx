namespace Backend.Fx.Tests.Exceptions
{
    using System;
    using BuildingBlocks;
    using Fx.Exceptions;
    using Xunit;

    public class TheUnprocessableExceptionBuilder
    {
        [Fact]
        public void CatchesExceptionInAction()
        {
            var sut = UnprocessableException.UseBuilder();
            sut.CatchPossibleException(() => { throw new InvalidOperationException("hello"); });
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void CatchesExceptionInFunc()
        {
            var sut = new UnprocessableExceptionBuilder();
            sut.CatchPossibleException<int>(() => { throw new InvalidOperationException("hello"); });
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void AddsExceptionWhenAggregateIsNull()
        {
            var sut = new UnprocessableExceptionBuilder();
            sut.AddNotFoundWhenNull<TheAggregateRoot.TestAggregateRoot>(1111, null);
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void AddsNoExceptionWhenAggregateIsNotNull()
        {
            var sut = new UnprocessableExceptionBuilder();
            sut.AddNotFoundWhenNull(1111, new TheAggregateRoot.TestAggregateRoot("gaga"));
            sut.Dispose();
        }

        [Fact]
        public void AddsExceptionWhenAddingError()
        {
            var sut = new UnprocessableExceptionBuilder();
            sut.Add("something is broken");
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void AddsExceptionWhenAddingConditionalError()
        {
            var sut = new UnprocessableExceptionBuilder();
            sut.AddIf(true, "something is broken");
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void AddsExceptionWhenNotAddingConditionalError()
        {
            var sut = new UnprocessableExceptionBuilder();
            sut.AddIf(false, "something is broken");
            sut.Dispose();
        }
    }
}
