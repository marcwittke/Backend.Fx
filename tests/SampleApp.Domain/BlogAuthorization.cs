using Backend.Fx.Features.Authorization;
using JetBrains.Annotations;

namespace SampleApp.Domain
{
    [UsedImplicitly]
    public class BlogAuthorization : AllowAll<Blog>
    {
    }
}