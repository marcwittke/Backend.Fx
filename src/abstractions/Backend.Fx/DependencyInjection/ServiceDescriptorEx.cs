using Backend.Fx.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.DependencyInjection
{
    [PublicAPI]
    public static class ServiceDescriptorEx
    {
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