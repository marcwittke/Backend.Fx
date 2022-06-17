using Backend.Fx.Patterns.Authorization;
using JetBrains.Annotations;

namespace SampleApp.Domain
{
    [UsedImplicitly]
    public class BlogAuthorization : AllowAll<Blog>
    {
    }
}