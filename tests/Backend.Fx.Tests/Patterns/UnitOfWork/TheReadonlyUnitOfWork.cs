using Backend.Fx.Patterns.UnitOfWork;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.UnitOfWork
{
    public class TheReadonlyDecorator
    {
        [Fact]
        public void RollsBackOnComplete()
        {
            IUnitOfWork uow = A.Fake<IUnitOfWork>();
            IUnitOfWork sut = new ReadonlyDecorator(uow);
            sut.Begin();
            sut.Complete();
            sut.Dispose();
            A.CallTo(() => uow.Complete()).MustNotHaveHappened();
            A.CallTo(() => uow.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void RollsBackOnDispose()
        {
            IUnitOfWork uow = A.Fake<IUnitOfWork>();
            IUnitOfWork sut = new ReadonlyDecorator(uow);
            sut.Begin();
            sut.Dispose();
            A.CallTo(() => uow.Complete()).MustNotHaveHappened();
            A.CallTo(() => uow.Dispose()).MustHaveHappenedOnceExactly();
        }
    }
}
