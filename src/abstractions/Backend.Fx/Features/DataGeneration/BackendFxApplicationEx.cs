using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.DataGeneration
{
    public static class BackendFxApplicationEx
    {
        public static async Task GenerateData(this IBackendFxApplication application, CancellationToken cancellationToken = default)
        {
            var dataGenerationContext = application.CompositionRoot.ServiceProvider.GetRequiredService<IDataGenerationContext>();
            var dataGeneratorTypes = await dataGenerationContext.GetDataGeneratorTypesAsync(application.Invoker).ConfigureAwait(false);
            await dataGenerationContext.GenerateDataAsync(application.Invoker, dataGeneratorTypes, cancellationToken).ConfigureAwait(false);
        }
    }
}