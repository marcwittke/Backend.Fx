using Backend.Fx.Patterns.Authorization;
using JetBrains.Annotations;

namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain
{
    [UsedImplicitly]
    public class BloggerAuthorization : AllowAll<Blogger>
    {
    }
}