using System.Security.Principal;
using JetBrains.Annotations;

namespace Backend.Fx.ExecutionPipeline
{
    [PublicAPI]
    public struct SystemIdentity : IIdentity
    {
        public string Name => "SYSTEM";

        public string AuthenticationType => "Internal";

        public bool IsAuthenticated => true;

        public override bool Equals(object obj)
        {
            return obj is SystemIdentity;
        }

        public override int GetHashCode()
        {
            return 542451621;
        }

        public bool Equals(SystemIdentity other)
        {
            return true;
        }
    }
}