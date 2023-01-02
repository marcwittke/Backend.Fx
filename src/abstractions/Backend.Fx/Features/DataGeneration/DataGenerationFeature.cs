using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Backend.Fx.Features.DataGeneration
{
    /// <summary>
    /// The extension "Data Generation" makes sure that all implemented data generators are executed on application boot
    /// </summary>
    [PublicAPI]
    public class DataGenerationFeature : Feature, IBootableFeature, IMultiTenancyFeature
    {
        private readonly bool _allowDemoDataGeneration;

        /// <param name="allowDemoDataGeneration">
        /// Controls, whether demo data generators should run. In case of a multi tenancy application, this flag must
        /// be set to <c>true</c> to allow specific tenants to contain demo data, following the respective tenant
        /// configuration.  
        /// </param>
        public DataGenerationFeature(bool allowDemoDataGeneration = true)
        {
            _allowDemoDataGeneration = allowDemoDataGeneration;
        }

        public override void Enable(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(
                new DataGenerationModule(application.Assemblies, _allowDemoDataGeneration));
        }

        public void EnableMultiTenancyServices(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(new MultiTenancyDataGenerationModule());
        }
        
        public async Task BootAsync(IBackendFxApplication application, CancellationToken cancellationToken = default)
        {
            await application.GenerateData(cancellationToken).ConfigureAwait(false);
        }
    }
}