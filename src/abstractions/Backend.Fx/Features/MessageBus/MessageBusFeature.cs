using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Features.MessageBus
{
    /// <summary>
    /// The extension "Message Bus" adds integration message sending and handling of received integration messages to the
    /// application. If the feature "Multi Tenancy" has been activated, this feature takes care of adding a tenant id
    /// to all outgoing messages and handling incoming messages in the respective tenant. 
    /// </summary>
    public class MessageBusFeature : Feature, IBootableFeature, IMultiTenancyFeature
    {
        private readonly IMessageBus _messageBus;
        private MessageBusModule _messageBusModule;

        public MessageBusFeature(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        public override void Enable(IBackendFxApplication application)
        {
            _messageBus.Integrate(application);
            _messageBusModule = new MessageBusModule(_messageBus, application);
            application.CompositionRoot.RegisterModules(_messageBusModule);
        }

        public Task BootAsync(IBackendFxApplication application, CancellationToken cancellationToken = default)
        {
            _messageBus.Connect();
            _messageBusModule.SubscribeToAllEvents();
            return Task.CompletedTask;
        }

        public void EnableMultiTenancyServices(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(new MultiTenancyMessageBusModule());
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