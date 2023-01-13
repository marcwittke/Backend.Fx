using System;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Features;
using Backend.Fx.SimpleInjectorDependencyInjection;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.TestUtil;

public class MockFeature : Feature
{
    private ICompositionRoot _compositionRoot;

    public override void Enable(IBackendFxApplication application)
    {
        _compositionRoot = application.CompositionRoot;
        if (_compositionRoot is SimpleInjectorCompositionRoot simpleInjectorCompositionRoot)
        {
            simpleInjectorCompositionRoot.Container.Options.AllowOverridingRegistrations = true;
        }
    }

    public T AddMock<T>(T mock = null) where T : class
    {
        if (_compositionRoot == null) throw new InvalidOperationException("You have to enable the MockFeature first");

        mock ??= A.Fake<T>();
        
        _compositionRoot.Register(ServiceDescriptor.Scoped(_ => mock));
        return mock;
    }
}