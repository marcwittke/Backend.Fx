﻿using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.Transactions
{
    public class TheReadonlyDecorator
    {
        private readonly IOperation _operation;
        private readonly IOperation _sut;

        public TheReadonlyDecorator()
        {
            _operation = A.Fake<IOperation>();
            _sut = new ReadonlyDbTransactionOperationDecorator(_operation);
        }
        [Fact]
        public void DelegatesOtherCalls()
        {
            _sut.Begin();
            A.CallTo(() => _operation.Begin()).MustHaveHappenedOnceExactly();

            _sut.Cancel();
            A.CallTo(() => _operation.Cancel()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CancelsOperationOnComplete()
        {
            _sut.Begin();
            _sut.Complete();
            A.CallTo(() => _operation.Complete()).MustNotHaveHappened();
            A.CallTo(() => _operation.Cancel()).MustHaveHappenedOnceExactly();
        }
    }
}