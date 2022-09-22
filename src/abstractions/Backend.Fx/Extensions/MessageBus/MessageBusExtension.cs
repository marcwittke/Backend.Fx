using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Extensions.MessageBus
{
    /// <summary>
    /// The extension "Message Bus" adds integration message sending and handling of received integration messages to the
    /// application. If the feature "Multi Tenancy" has been activated, this feature takes care of adding a tenant id
    /// to all outgoing messages and handling incoming messages in the respective tenant. 
    /// </summary>
    public class MessageBusExtension : BackendFxApplicationExtension
    {
        private readonly MessageBus _messageBus;
        private readonly MessageBusModule _messageBusModule;

        public MessageBusExtension(MessageBus messageBus, IBackendFxApplication application)
            : base(application)
        {
            _messageBusModule = new MessageBusModule(messageBus, application);
            application.CompositionRoot.RegisterModules(_messageBusModule);
            _messageBus = messageBus;
            
        }

        public override async Task BootAsync(CancellationToken cancellationToken = default)
        {
            await base.BootAsync(cancellationToken).ConfigureAwait(false);
            _messageBus.Connect();
            _messageBusModule.SubscribeToAllEvents();
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