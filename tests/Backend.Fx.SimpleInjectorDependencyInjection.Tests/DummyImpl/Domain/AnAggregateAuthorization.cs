using Backend.Fx.Patterns.Authorization;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Domain
{
    public class AnAggregateAuthorization : AllowAll<AnAggregate> { }
}
