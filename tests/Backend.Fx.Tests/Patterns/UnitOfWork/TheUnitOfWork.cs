namespace Backend.Fx.Tests.Patterns.UnitOfWork
{
    using FakeItEasy;
    using Fx.Environment.Authentication;
    using Fx.Environment.DateAndTime;
    using Fx.Patterns.EventAggregation.Domain;
    using Xunit;

    public class TheUnitOfWork
    {
        private readonly IDomainEventAggregator eventAggregator = A.Fake<IDomainEventAggregator>();

        [Fact]
        public void CommitsOnComplete()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), eventAggregator);
            sut.Begin();
            sut.Complete();
            sut.Dispose();
            Assert.Equal(0, sut.RollbackCount);
            Assert.Equal(1, sut.UpdateTrackingPropertiesCount);
            Assert.Equal(1, sut.CommitCount);
        }

        [Fact]
        public void RaisesEventsOnComplete()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), eventAggregator);
            sut.Begin();
            sut.Complete();
            sut.Dispose();
            A.CallTo(() => eventAggregator.RaiseEvents()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void RollsBackOnDispose()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), eventAggregator);
            sut.Begin();
            sut.Dispose();
            Assert.Equal(1, sut.RollbackCount);
            Assert.Equal(0, sut.UpdateTrackingPropertiesCount);
            Assert.Equal(0, sut.CommitCount);
        }

        [Fact]
        public void RaisesNoEventsOnDispose()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), eventAggregator);
            sut.Begin();
            sut.Dispose();
            A.CallTo(() => eventAggregator.RaiseEvents()).MustNotHaveHappened();
        }

        [Fact]
        public void UpdatesTrackingPropertiesOnFlush()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), eventAggregator);
            sut.Begin();
            sut.Flush();
            Assert.Equal(0, sut.RollbackCount);
            Assert.Equal(1, sut.UpdateTrackingPropertiesCount);
            Assert.Equal(0, sut.CommitCount);
        }
    }
}