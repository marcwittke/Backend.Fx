using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Tests.Patterns.DependencyInjection;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class TheMessageBusApplication
    {
        [Fact]
        public async Task Boots()
        {
            var app = new MessageBusTestApplication(CompositionRootType.Microsoft);
            await app.BootAsync();
        }
    }

    public class MessageBusTestApplication : MessageBusApplication
    {
        public MessageBusTestApplication(CompositionRootType compositionRootType)
            : base(
                new InMemoryMessageBus(),
                new BackendFxApplication(
                    compositionRootType.Create(),
                    A.Fake<IExceptionLogger>(),
                    typeof(MessageBusTestApplication).Assembly))
        {
        }
    }
}