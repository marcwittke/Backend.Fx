using System.Threading.Tasks;
using Backend.Fx.Extensions.MessageBus;
using Backend.Fx.Extensions.MessageBus.InProc;
using Backend.Fx.Logging;
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
            var app = new MessageBusTestExtension(compositionRootType);
            await app.BootAsync();
        }
    }

    public class MessageBusTestExtension : MessageBusExtension
    {
        public MessageBusTestExtension(CompositionRootType compositionRootType)
            : base(
                new InProcMessageBus(),
                new BackendFxApplication(
                    compositionRootType.Create(),
                    A.Fake<IExceptionLogger>()))
        {
        }
    }
}