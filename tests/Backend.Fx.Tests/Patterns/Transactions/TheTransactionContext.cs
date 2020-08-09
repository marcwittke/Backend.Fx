using System;
using System.Data;
using Backend.Fx.Patterns.Transactions;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.Transactions
{
    public class TheTransactionContext
    {
        public TheTransactionContext()
        {
            _sut = new TransactionContext(_dbConnection);
            A.CallTo(() => _dbConnection.BeginTransaction(A<IsolationLevel>._)).Returns(_dbTransaction);
        }

        private readonly IDbConnection _dbConnection = A.Fake<IDbConnection>();
        private readonly IDbTransaction _dbTransaction = A.Fake<IDbTransaction>();
        private readonly TransactionContext _sut;

        [Fact]
        public void BeginsTransactionInSpecificIsolationLevel()
        {
            _sut.SetIsolationLevel(IsolationLevel.ReadCommitted);
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            _sut.BeginTransaction();
            A.CallTo(() => _dbConnection.BeginTransaction(A<IsolationLevel>.That.IsEqualTo(IsolationLevel.ReadCommitted))).MustHaveHappenedOnceExactly();
            Assert.Equal(_sut.CurrentTransaction, _dbTransaction);
        }

        [Fact]
        public void BeginsTransactionInUnspecifiedIsolationLevel()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            _sut.BeginTransaction();
            A.CallTo(() => _dbConnection.BeginTransaction(A<IsolationLevel>.That.IsEqualTo(IsolationLevel.Unspecified))).MustHaveHappenedOnceExactly();
            Assert.Equal(_sut.CurrentTransaction, _dbTransaction);
        }

        [Fact]
        public void CannotInitializeWithConnectionInWrongState()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Broken);
            Assert.Throws<InvalidOperationException>(() => new TransactionContext(_dbConnection));

            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Connecting);
            Assert.Throws<InvalidOperationException>(() => new TransactionContext(_dbConnection));

            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Executing);
            Assert.Throws<InvalidOperationException>(() => new TransactionContext(_dbConnection));

            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Fetching);
            Assert.Throws<InvalidOperationException>(() => new TransactionContext(_dbConnection));
        }

        [Fact]
        public void ClosesAndDisposesOnDispose()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            _sut.BeginTransaction();
            _sut.Dispose();
            A.CallTo(() => _dbConnection.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dbTransaction.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void DisposesOnDispose()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            var sut = new TransactionContext(_dbConnection);
            sut.BeginTransaction();
            sut.Dispose();
            A.CallTo(() => _dbConnection.Close()).MustNotHaveHappened();
            A.CallTo(() => _dbTransaction.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void DoesNotAllowToChangeIsolationLevenWhenBegun()
        {
            _sut.SetIsolationLevel(IsolationLevel.ReadCommitted);
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            _sut.BeginTransaction();
            Assert.Throws<InvalidOperationException>(() => _sut.SetIsolationLevel(IsolationLevel.Chaos));
        }

        [Fact]
        public void DoesNotCommitOnRollback()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            var sut = new TransactionContext(_dbConnection);
            sut.BeginTransaction();
            A.CallTo(() => _dbConnection.Open()).MustNotHaveHappened();
            sut.RollbackTransaction();
            A.CallTo(() => _dbTransaction.Commit()).MustNotHaveHappened();
            A.CallTo(() => _dbTransaction.Rollback()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void DoesNotMaintainConnectionStateWhenProvidingOpenConnection()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            var sut = new TransactionContext(_dbConnection);
            sut.BeginTransaction();
            A.CallTo(() => _dbConnection.Open()).MustNotHaveHappened();
            sut.CommitTransaction();
            A.CallTo(() => _dbConnection.Close()).MustNotHaveHappened();

            Fake.ClearRecordedCalls(_dbConnection);

            sut = new TransactionContext(_dbConnection);
            sut.BeginTransaction();
            A.CallTo(() => _dbConnection.Open()).MustNotHaveHappened();
            sut.RollbackTransaction();
            A.CallTo(() => _dbConnection.Close()).MustNotHaveHappened();
        }

        [Fact]
        public void DoesNotRollbackOnCommit()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            var sut = new TransactionContext(_dbConnection);
            sut.BeginTransaction();
            A.CallTo(() => _dbConnection.Open()).MustNotHaveHappened();
            sut.CommitTransaction();
            A.CallTo(() => _dbTransaction.Rollback()).MustNotHaveHappened();
            A.CallTo(() => _dbTransaction.Commit()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void HandlesDisposeException()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Closed);
            A.CallTo(() => _dbConnection.Close()).Throws<InvalidOperationException>();
            var sut = new TransactionContext(_dbConnection);
            sut.BeginTransaction();
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Open);
            sut.Dispose();
            A.CallTo(() => _dbConnection.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dbTransaction.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void InitializesWithoutCurrentTransaction()
        {
            Assert.Null(_sut.CurrentTransaction);
        }

        [Fact]
        public void MaintainsConnectionStateWhenProvidingClosedConnection()
        {
            A.CallTo(() => _dbConnection.State).Returns(ConnectionState.Closed);
            var sut = new TransactionContext(_dbConnection);
            sut.BeginTransaction();
            A.CallTo(() => _dbConnection.Open()).MustHaveHappenedOnceExactly();
            sut.CommitTransaction();
            A.CallTo(() => _dbConnection.Close()).MustHaveHappenedOnceExactly();

            Fake.ClearRecordedCalls(_dbConnection);

            sut = new TransactionContext(_dbConnection);
            sut.BeginTransaction();
            A.CallTo(() => _dbConnection.Open()).MustHaveHappenedOnceExactly();
            sut.RollbackTransaction();
            A.CallTo(() => _dbConnection.Close()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void UsesGivenConnection()
        {
            Assert.Equal(_dbConnection, _sut.Connection);
        }
    }
}