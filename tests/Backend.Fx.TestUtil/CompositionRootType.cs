using System;
using Backend.Fx.DependencyInjection;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.SimpleInjectorDependencyInjection;

namespace Backend.Fx.TestUtil
{
    public enum CompositionRootType
    {
        Microsoft,
        SimpleInjector
    }
    
    public static class CompositionRootTypeEx
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