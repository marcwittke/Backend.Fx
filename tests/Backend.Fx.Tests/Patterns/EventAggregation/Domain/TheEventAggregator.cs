using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Patterns.EventAggregation.Domain;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Domain
{
    public class TheEventAggregator : TestWithLogging
    {
        [Fact]
        public void CallsAllDomainEventHandlers()
        {
            var handler1 = new TestDomainEventHandler();
            var handler2 = new TestDomainEventHandler();
            var fakeServiceProvider = A.Fake<IServiceProvider>();
            A.CallTo(() => fakeServiceProvider
                    .GetService(A<Type>.That.IsEqualTo(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>))))
                .Returns(new[] {handler1, handler2}.AsEnumerable());

            IDomainEventAggregator sut = new DomainEventAggregator(fakeServiceProvider);

            sut.PublishDomainEvent(new TestDomainEvent(4711));
            sut.RaiseEvents();

            Assert.Single(handler1.Events);
            Assert.Equal(4711, handler1.Events[0].Id);

            Assert.Single(handler2.Events);
            Assert.Equal(4711, handler2.Events[0].Id);

            Assert.Equal(handler1.Events[0], handler2.Events[0]);
        }

        [Fact]
        public void DoesNotSwallowExceptionOnDomainEventHandling()
        {
            IDomainEventHandler<TestDomainEvent> handler = new FailingDomainEventHandler();
            var fakeServiceProvider = A.Fake<IServiceProvider>();
            A.CallTo(() => fakeServiceProvider
                    .GetService(A<Type>.That.IsEqualTo(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>))))
                .Returns(new[] {handler}.AsEnumerable());

            IDomainEventAggregator sut = new DomainEventAggregator(fakeServiceProvider);
            sut.PublishDomainEvent(new TestDomainEvent(444));
            Assert.Throws<NotSupportedException>(() => sut.RaiseEvents());
        }

        public TheEventAggregator(ITestOutputHelper output) : base(output)
        {
        }
    }
}