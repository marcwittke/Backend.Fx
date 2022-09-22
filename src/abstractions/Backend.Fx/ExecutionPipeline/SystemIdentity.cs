using System.Security.Principal;
using JetBrains.Annotations;

namespace Backend.Fx.ExecutionPipeline
{
    [PublicAPI]
    public sealed class SystemIdentity : IIdentity
    {
        public string Name => "SYSTEM";

        public string AuthenticationType => "system internal";

        public bool IsAuthenticated => true;
    }
}