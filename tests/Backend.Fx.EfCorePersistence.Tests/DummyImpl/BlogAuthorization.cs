namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl
{
    using Patterns.Authorization;
    public class BlogAuthorization : AllowAll<Blog>
    { }
}
