using Backend.Fx.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public static class ServiceDescriptorEx
    {
        public static void LogDetails(this ServiceDescriptor serviceDescriptor, ILogger logger, string prefix = null)
        {
            logger.LogDebug("{Prefix} {Lifetime} registration for {ServiceType}: {ImplementationType}",
                prefix,
                serviceDescriptor.Lifetime.ToString(),
                serviceDescriptor.ServiceType.GetDetailedTypeName(),
                serviceDescriptor.GetImplementationTypeDescription());
        }

        public static string GetImplementationTypeDescription(this ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor.ImplementationFactory != null)
            {
                return serviceDescriptor.ImplementationFactory.GetType().GetDetailedTypeName();
            }

            if (serviceDescriptor.ImplementationType != null)
            {
                return serviceDescriptor.ImplementationType.GetDetailedTypeName();
            }

            if (serviceDescriptor.ImplementationInstance != null)
            {
                return serviceDescriptor.ImplementationInstance.GetType().GetDetailedTypeName();
            }

            return "Unknown";
        }
    }
}