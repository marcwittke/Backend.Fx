using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.MicrosoftDependencyInjection
{
    [PublicAPI]
    public class SharedMicrosoftCompositionRoot : MicrosoftCompositionRootBase
    {
        private IServiceProvider _serviceProvider;

        public SharedMicrosoftCompositionRoot(IServiceCollection serviceCollection) : base(serviceCollection)
        { }

        public override IServiceProvider ServiceProvider => 
            _serviceProvider ?? throw new InvalidOperationException("ServiceProvider not in use. Call UseServiceProvider(app.ServiceProvider) in Startup.Configure");

        public override void Verify()
        {
            // out of our control
        }

        public void UseServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
    }
}