using System;
using System.Data;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.Transactions
{
    public class TheDbTransactionOperationDecorator : TestWithLogging
    {
        public TheDbTransactionOperationDecorator(ITestOutputHelper output): base(output)
        {
            _sut = new DbTransactionOperationDecorator(_dbConnection, _operation);
            A.CallTo(() => _dbConnection.BeginTransaction(A<IsolationLevel>._)).Returns(_dbTransaction);
        }

        private readonly IDbConnection _dbConnection = A.Fake<IDbConnection>();
        private readonly IDbTransaction _dbTransaction = A.Fake<IDbTransaction>();
        private readonly IOperation _operation = new Operation();
        private readonly DbTransactionOperationDecorator _sut;

        [Theory]
        [InlineData(IsolationLevel.ReadCommitted)]
        [InlineData(IsolationLevel.ReadUncommitted)]
        [InlineData(IsolationLevel.Serializable)]
        public void BeginsTransactionInSpecificIsolationLevel(IsolationLevel isolationLevel)
        {
            _sut.SetIsolationLevel(isolationLevel);
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            _sut.Begin();
            A.CallTo(() => _dbConnection.BeginTransaction(A<IsolationLevel>.That.IsEqualTo(isolationLevel))).MustHaveHappenedOnceExactly();
            Assert.Equal(_sut.CurrentTransaction, _dbTransaction);
        }

        [Fact]
        public void BeginsTransactionInUnspecifiedIsolationLevel()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            _sut.Begin();
            A.CallTo(() => _dbConnection.BeginTransaction(A<IsolationLevel>.That.IsEqualTo(IsolationLevel.Unspecified))).MustHaveHappenedOnceExactly();
            Assert.Equal(_sut.CurrentTransaction, _dbTransaction);
        }

        [Theory]
        [InlineData(ConnectionState.Broken)]
        [InlineData(ConnectionState.Connecting)]
        [InlineData(ConnectionState.Executing)]
        [InlineData(ConnectionState.Fetching)]
        public void CannotBeginWithConnectionIsInWrongState(ConnectionState wrongState)
        {
            A.CallTo(() => _dbConnection.State).Returns(wrongState);
            var sut = new DbTransactionOperationDecorator(_dbConnection, _operation);
            Assert.Throws<InvalidOperationException>(() => sut.Begin());
        }

        [Fact]
        public void ClosesAndDisposesOnCancel()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Closed);
            _sut.Begin();
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            _sut.Cancel();
            A.CallTo(() => _dbConnection.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dbTransaction.Dispose()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dbTransaction.Commit()).MustNotHaveHappened();
        }

        [Fact]
        public void DisposesOnCancel()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            _sut.Begin();
            _sut.Cancel();
            A.CallTo(() => _dbConnection.Close()).MustNotHaveHappened();
            A.CallTo(() => _dbTransaction.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void DoesNotAllowToChangeIsolationLevenWhenBegun()
        {
            _sut.SetIsolationLevel(IsolationLevel.ReadCommitted);
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            _sut.Begin();
            Assert.Throws<InvalidOperationException>(() => _sut.SetIsolationLevel(IsolationLevel.Chaos));
        }

        [Fact]
        public void DoesNotCommitbutRollbackOnCancel()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            _sut.Begin();
            A.CallTo(() => _dbConnection.Open()).MustNotHaveHappened();
            _sut.Cancel();
            A.CallTo(() => _dbTransaction.Commit()).MustNotHaveHappened();
            A.CallTo(() => _dbTransaction.Rollback()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void DoesNotMaintainConnectionStateOnCompleteWhenProvidingOpenConnection()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);

            _sut.Begin();
            A.CallTo(() => _dbConnection.Open()).MustNotHaveHappened();
            _sut.Complete();
            A.CallTo(() => _dbConnection.Close()).MustNotHaveHappened();
        }

        [Fact]
        public void DoesNotMaintainConnectionStateOnCancelWhenProvidingOpenConnection()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            
            _sut.Begin();
            A.CallTo(() => _dbConnection.Open()).MustNotHaveHappened();
            _sut.Cancel();
            A.CallTo(() => _dbConnection.Close()).MustNotHaveHappened();
        }

        [Fact]
        public void DoesNotRollbackButCommitOnComplete()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            _sut.Begin();
            A.CallTo(() => _dbConnection.Open()).MustNotHaveHappened();
            _sut.Complete();
            A.CallTo(() => _dbTransaction.Rollback()).MustNotHaveHappened();
            A.CallTo(() => _dbTransaction.Commit()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void InitializesWithoutCurrentTransaction()
        {
            Assert.Null(_sut.CurrentTransaction);
        }

        [Fact]
        public void MaintainsConnectionStateOnCompleteWhenProvidingClosedConnection()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Closed);
            _sut.Begin();
            A.CallTo(() => _dbConnection.Open()).MustHaveHappenedOnceExactly();
            _sut.Complete();
            A.CallTo(() => _dbConnection.Close()).MustHaveHappenedOnceExactly();

        }

        [Fact]
        public void MaintainsConnectionStateOnCancelWhenProvidingClosedConnection()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Closed);

            _sut.Begin();
            A.CallTo(() => _dbConnection.Open()).MustHaveHappenedOnceExactly();
            _sut.Cancel();
            A.CallTo(() => _dbConnection.Close()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void UsesGivenConnection()
        {
            _sut.Begin();
            A.CallTo(() => _dbConnection.BeginTransaction(A<IsolationLevel>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void DoesNotAllowBeginWhenActive()
        {
            _sut.Begin();
            Assert.Throws<InvalidOperationException>(() => _sut.Begin());
        }

        [Fact]
        public void DoesNotAllowCompleteWhenNotStarted()
        {
            Assert.Throws<InvalidOperationException>(() => _sut.Complete());
        }

        [Fact]
        public void DoesNotAllowCancelWhenNotStarted()
        {
            Assert.Throws<InvalidOperationException>(() => _sut.Cancel());
        }

        [Fact]
        public void DoesNotAllowCancelWhenCompleted()
        {
            _sut.Begin();
            _sut.Complete();
            Assert.Throws<InvalidOperationException>(() => _sut.Cancel());
        }

        [Fact]
        public void DoesNotAllowCompleteWhenRolledBack()
        {
            _sut.Begin();
            _sut.Cancel();
            Assert.Throws<InvalidOperationException>(() => _sut.Complete());
        }
    }
}