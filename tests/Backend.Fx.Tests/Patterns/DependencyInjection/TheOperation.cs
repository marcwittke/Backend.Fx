using System;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheOperation : TestWithLogging
    {
        public TheOperation(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CanCancel()
        {
            var sut = new Operation();
            sut.Begin();
            sut.Cancel();
        }

        [Fact]
        public void CanComplete()
        {
            var sut = new Operation();
            sut.Begin();
            sut.Complete();
        }

        [Fact]
        public void CannotCompleteCanceled()
        {
            var sut = new Operation();
            sut.Begin();
            sut.Cancel();
            Assert.Throws<InvalidOperationException>(() => sut.Complete());
        }

        [Fact]
        public void CannotBeginCanceled()
        {
            var sut = new Operation();
            sut.Begin();
            sut.Cancel();
            Assert.Throws<InvalidOperationException>(() => sut.Begin());
        }
        
        [Fact]
        public void CannotCancelNew()
        {
            var sut = new Operation();
            Assert.Throws<InvalidOperationException>(() => sut.Cancel());
        }
        
        [Fact]
        public void CannotCompleteNew()
        {
            var sut = new Operation();
            Assert.Throws<InvalidOperationException>(() => sut.Complete());
        }

        [Fact]
        public void CannotCancelCompleted()
        {
            var sut = new Operation();
            sut.Begin();
            sut.Complete();
            Assert.Throws<InvalidOperationException>(() => sut.Cancel());
        }

        [Fact]
        public void CannotBeginCompleted()
        {
            var sut = new Operation();
            sut.Begin();
            sut.Complete();
            Assert.Throws<InvalidOperationException>(() => sut.Begin());
        }

        [Fact]
        public void CannotBeginTwice()
        {
            var sut = new Operation();
            sut.Begin();
            Assert.Throws<InvalidOperationException>(() => sut.Begin());
        }

        [Fact]
        public void CannotCompleteTwice()
        {
            var sut = new Operation();
            sut.Begin();
            sut.Complete();
            Assert.Throws<InvalidOperationException>(() => sut.Complete());
        }

        [Fact]
        public void CannotCancelTwice()
        {
            var sut = new Operation();
            sut.Begin();
            sut.Cancel();
            Assert.Throws<InvalidOperationException>(() => sut.Cancel());
        }
    }
}