using Xunit;

namespace Backend.Fx.Tests.Exceptions
{
    using System;
    using BuildingBlocks;
    using Fx.Exceptions;

    public class TheUnprocessableExceptionBuilder
    {
        [Fact]
        public void CatchesExceptionInAction()
        {
            var sut = UnprocessableException.UseBuilder();
            sut.CatchPossibleException(() => throw new InvalidOperationException("hello"));
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void CatchesExceptionInFunc()
        {
            var sut = UnprocessableException.UseBuilder();
            sut.CatchPossibleException<int>(() => throw new InvalidOperationException("hello"));
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void AddsExceptionWhenAggregateIsNull()
        {
            var sut = UnprocessableException.UseBuilder();
            sut.AddNotFoundWhenNull<TheAggregateRoot.TestAggregateRoot>(1111, null);
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void AddsNoExceptionWhenAggregateIsNotNull()
        {
            var sut = UnprocessableException.UseBuilder();
            sut.AddNotFoundWhenNull(1111, new TheAggregateRoot.TestAggregateRoot(12345, "gaga"));
            sut.Dispose();
        }

        [Fact]
        public void ThrowsExceptionWhenAddingError()
        {
            var sut = UnprocessableException.UseBuilder();
            sut.Add(new Error("code", "something is broken"));
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void ThrowsExceptionWhenAddingConditionalError()
        {
            var sut = UnprocessableException.UseBuilder();
            sut.AddIf(true, new Error("code", "something is broken"));
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void DoesNotThrowExceptionWhenNotAddingConditionalError()
        {
            var sut = UnprocessableException.UseBuilder();
            sut.AddIf(false, new Error("code", "something is broken"));
            sut.Dispose();
        }
    }
}
