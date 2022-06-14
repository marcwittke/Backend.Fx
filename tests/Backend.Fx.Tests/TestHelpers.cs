using System;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.Tests.Patterns.DependencyInjection;

namespace Backend.Fx.Tests
{
    public static class TestHelpers
    {
        public static ICompositionRoot Create(this CompositionRootType compositionRootType)
        {
            return compositionRootType switch
            {
                CompositionRootType.Microsoft => new MicrosoftCompositionRoot(),
                CompositionRootType.SimpleInjector => new SimpleInjectorCompositionRoot(),
                _ => throw new ArgumentException(nameof(compositionRootType))
            };
        }
    }
}