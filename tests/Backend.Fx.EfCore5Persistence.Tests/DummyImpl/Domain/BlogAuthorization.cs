using Backend.Fx.Patterns.Authorization;
using JetBrains.Annotations;

namespace Backend.Fx.EfCore5Persistence.Tests.DummyImpl.Domain
{
    [UsedImplicitly]
    public class BlogAuthorization : AllowAll<Blog>
    {
    }
}