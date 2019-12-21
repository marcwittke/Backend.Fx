using Backend.Fx.Patterns.Transactions;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.UnitOfWork
{
    public class TheReadonlyDecorator
    {
        [Fact]
        public void RollsBackOnComplete()
        {
            var transactionContext = A.Fake<ITransactionContext>();
            ITransactionContext sut = new ReadonlyDecorator(transactionContext);
            sut.BeginTransaction();
            sut.CommitTransaction();
            A.CallTo(() => transactionContext.CommitTransaction()).MustNotHaveHappened();
        }
    }
}
