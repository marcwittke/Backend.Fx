using System.Security.Principal;
using JetBrains.Annotations;

namespace Backend.Fx.ExecutionPipeline
{
    [PublicAPI]
    public sealed class AnonymousIdentity : IIdentity
    {
        public string Name => "ANONYMOUS";

        public string AuthenticationType => string.Empty;

        public bool IsAuthenticated => false;
    }
}