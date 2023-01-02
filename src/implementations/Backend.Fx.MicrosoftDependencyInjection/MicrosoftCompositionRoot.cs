using System;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.MicrosoftDependencyInjection
{
    [PublicAPI]
    public class MicrosoftCompositionRoot : MicrosoftCompositionRootBase
    {
        private readonly ILogger _logger = Log.Create<MicrosoftCompositionRoot>();
        private readonly Lazy<IServiceProvider> _serviceProvider;

        public MicrosoftCompositionRoot()
        {
            _serviceProvider = new Lazy<IServiceProvider>(() =>
            {
                _logger.LogInformation("Building Microsoft ServiceProvider");
                return ServiceCollection.BuildServiceProvider(
                    new ServiceProviderOptions
                    {
                        ValidateScopes = true,
                        ValidateOnBuild = true
                    });
            });
        }

        public override IServiceProvider ServiceProvider => _serviceProvider.Value;

        public override void Verify()
        {
            // ensure creation of lazy service provider, this will trigger the validation
            var unused = _serviceProvider.Value;
        }
    }
}