using Backend.Fx.Patterns.Jobs;
using JetBrains.Annotations;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl
{
    [UsedImplicitly]
    public class SomeJob : IJob
    {
        public void Run()
        {}
    }
}
