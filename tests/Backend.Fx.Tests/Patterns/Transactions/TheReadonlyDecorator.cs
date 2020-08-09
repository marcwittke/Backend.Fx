using System.Data;
using Backend.Fx.Patterns.Transactions;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.Transactions
{
    public class TheReadonlyDecorator
    {
        [Fact]
        public void DelegatesOtherCalls()
        {
            var transactionContext = A.Fake<ITransactionContext>();
            ITransactionContext sut = new ReadonlyDecorator(transactionContext);

            // ReSharper disable once UnusedVariable
            IDbTransaction x = sut.CurrentTransaction;
            A.CallTo(() => transactionContext.CurrentTransaction).MustHaveHappenedOnceExactly();

            sut.SetIsolationLevel(IsolationLevel.Chaos);
            A.CallTo(() => transactionContext.SetIsolationLevel(A<IsolationLevel>.That.IsEqualTo(IsolationLevel.Chaos))).MustHaveHappenedOnceExactly();
        }

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