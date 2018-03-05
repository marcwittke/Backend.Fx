namespace Backend.Fx.Tests.Patterns.UnitOfWork
{
    using Fx.Environment.Authentication;
    using Fx.Environment.DateAndTime;
    using Xunit;

    public class TheUnitOfWork
    {
        [Fact]
        public void CommitsBackOnComplete()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem());
            sut.Begin();
            sut.Complete();
            sut.Dispose();
            Assert.Equal(0, sut.RollbackCount);
            Assert.Equal(1, sut.UpdateTrackingPropertiesCount);
            Assert.Equal(1, sut.CommitCount);
        }

        [Fact]
        public void RollsBackOnDispose()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem());
            sut.Begin();
            sut.Dispose();
            Assert.Equal(1, sut.RollbackCount);
            Assert.Equal(0, sut.UpdateTrackingPropertiesCount);
            Assert.Equal(0, sut.CommitCount);
        }

        [Fact]
        public void UpdatesTrackingPropertiesOnFlush()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem());
            sut.Begin();
            sut.Flush();
            Assert.Equal(0, sut.RollbackCount);
            Assert.Equal(1, sut.UpdateTrackingPropertiesCount);
            Assert.Equal(0, sut.CommitCount);
        }
    }
}