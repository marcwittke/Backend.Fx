using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public class MessageBusApplication : BackendFxApplicationDecorator
    {
        private readonly IMessageBus _messageBus;

        public MessageBusApplication(IMessageBus messageBus, IBackendFxApplication application)
            : base(application)
        {
            application.CompositionRoot.RegisterModules(new MessageBusModule(messageBus, application.Assemblies));
            _messageBus = messageBus;
            _messageBus.ProvideInvoker(
                new SequentializingBackendFxApplicationInvoker(
                    new ExceptionLoggingAndHandlingInvoker(application.ExceptionLogger, application.Invoker)));
        }

        public override async Task BootAsync(CancellationToken cancellationToken = default)
        {
            await base.BootAsync(cancellationToken).ConfigureAwait(false);
            _messageBus.Connect();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _messageBus.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}