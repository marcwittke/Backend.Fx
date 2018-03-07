namespace Backend.Fx.Tests.Patterns.EventAggregation
{
    using System;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Fx.Patterns.EventAggregation;
    using Xunit;


    public class TheEventAggregator
    {
        [Fact]
        public void CallsAllDomainEventHandlers()
        {
            var handler1 = new TestDomainEventHandler();
            var handler2 = new TestDomainEventHandler();
            IEventHandlerProvider fakeEventHandlerProvider = A.Fake<IEventHandlerProvider>();
            A.CallTo(() => fakeEventHandlerProvider.GetAllEventHandlers<TestDomainEvent>()).Returns(new[] { handler1, handler2 });

            IEventAggregator sut = new EventAggregator(fakeEventHandlerProvider);

            sut.PublishDomainEvent(new TestDomainEvent(4711));

            A.CallTo(() => fakeEventHandlerProvider.GetAllEventHandlers<TestDomainEvent>()).MustHaveHappened(Repeated.Exactly.Once);

            Assert.Equal(1, handler1.Events.Count);
            Assert.Equal(4711, handler1.Events[0].Id);

            Assert.Equal(1, handler2.Events.Count);
            Assert.Equal(4711, handler2.Events[0].Id);

            Assert.Equal(handler1.Events[0], handler2.Events[0]);
        }

        [Fact]
        public void CallsAllIntegrationEventHandlers()
        {
            IEventHandlerProvider fakeEventHandlerProvider = A.Fake<IEventHandlerProvider>();
            IEventAggregator sut = new EventAggregator(fakeEventHandlerProvider);

            TestIntegrationEvent forwardedEvent1=null;
            TestIntegrationEvent forwardedEvent2=null;

            sut.SubscribeToIntegrationEvent<TestIntegrationEvent>(evt => forwardedEvent1 = evt);
            sut.SubscribeToIntegrationEvent<TestIntegrationEvent>(evt => forwardedEvent2 = evt);

            sut.PublishIntegrationEvent(new TestIntegrationEvent(4711, 22)).Wait();

            Assert.NotNull(forwardedEvent1);
            Assert.Equal(4711, forwardedEvent1.Id);
            Assert.Equal(22, forwardedEvent1.TenantId);

            Assert.NotNull(forwardedEvent2);
            Assert.Equal(4711, forwardedEvent2.Id);
            Assert.Equal(22, forwardedEvent2.TenantId);

            Assert.Equal(forwardedEvent1, forwardedEvent2);
        }

        [Fact]
        public async Task DoesNotSwallowExceptionOnDomainEventHandling()
        {
            IDomainEventHandler<TestDomainEvent> handler = new FailingDomainEventHandler();
            IEventHandlerProvider fakeEventHandlerProvider = A.Fake<IEventHandlerProvider>();
            A.CallTo(() => fakeEventHandlerProvider.GetAllEventHandlers<TestDomainEvent>()).Returns(new[] { handler });

            IEventAggregator sut = new EventAggregator(fakeEventHandlerProvider);
            await Assert.ThrowsAsync<NotSupportedException>(()=> sut.PublishDomainEvent(new TestDomainEvent(444)));
        }

        [Fact]
        public void SwallowsExceptionOnIntegrationEventHandling()
        {
            IEventHandlerProvider fakeEventHandlerProvider = A.Fake<IEventHandlerProvider>();
            IEventAggregator sut = new EventAggregator(fakeEventHandlerProvider);
            bool itHappened = false;
            sut.SubscribeToIntegrationEvent<TestIntegrationEvent>(evt =>
            {
                itHappened = true;
                throw new NotSupportedException("boum");
            });

            sut.PublishIntegrationEvent(new TestIntegrationEvent(4711, 22)).Wait();

            Assert.True(itHappened);
        }
    }
}
