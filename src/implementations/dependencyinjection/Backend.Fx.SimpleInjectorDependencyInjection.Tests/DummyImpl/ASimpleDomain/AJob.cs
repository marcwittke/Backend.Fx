using Backend.Fx.Patterns.Jobs;
using JetBrains.Annotations;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.ASimpleDomain
{
    [UsedImplicitly]
    public class AJob : IJob
    {
        public void Run()
        {}
    }
}
