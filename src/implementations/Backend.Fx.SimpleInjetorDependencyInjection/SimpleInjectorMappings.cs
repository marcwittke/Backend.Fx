using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection
{
    public static class SimpleInjectorMappings
    {
        public static Lifestyle MapLifestyle(this ServiceLifetime serviceLifetime)
        {
            switch (serviceLifetime)
            {
                case ServiceLifetime.Scoped: return Lifestyle.Scoped;
                case ServiceLifetime.Singleton: return Lifestyle.Singleton;
                case ServiceLifetime.Transient: return Lifestyle.Transient;
                default: throw new ArgumentException($"Unknown ServiceLifetime {serviceLifetime}");
            }
        }
    }
}