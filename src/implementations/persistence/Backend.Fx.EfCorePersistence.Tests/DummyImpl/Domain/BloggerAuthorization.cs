using Backend.Fx.Patterns.Authorization;

namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain
{
    public class BloggerAuthorization : AllowAll<Blogger>
    { }
}
