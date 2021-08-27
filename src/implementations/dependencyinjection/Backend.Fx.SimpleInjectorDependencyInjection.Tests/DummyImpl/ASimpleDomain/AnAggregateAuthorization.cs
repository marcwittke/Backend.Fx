using Backend.Fx.Patterns.Authorization;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.ASimpleDomain
{
    public class AnAggregateAuthorization : AllowAll<AnAggregate> { }
}
