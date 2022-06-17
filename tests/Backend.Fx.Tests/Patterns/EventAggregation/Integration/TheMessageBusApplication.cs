using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class TheMessageBusApplication
    {
        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task Boots(CompositionRootType compositionRootType)
        {
            var app = new MessageBusTestApplication(compositionRootType);
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
                    A.Fake<IExceptionLogger>()))
        {
        }
    }
}