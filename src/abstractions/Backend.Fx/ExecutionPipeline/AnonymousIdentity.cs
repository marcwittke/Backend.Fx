using System.Security.Principal;
using JetBrains.Annotations;

namespace Backend.Fx.ExecutionPipeline
{
    [PublicAPI]
    public readonly struct AnonymousIdentity : IIdentity
    {
        public string Name => "ANONYMOUS";

        public string AuthenticationType => null;

        public bool IsAuthenticated => false;
        
        public override bool Equals(object obj)
        {
            return obj is AnonymousIdentity;
        }

        public override int GetHashCode()
        {
            return 1564925492;
        }

        public bool Equals(AnonymousIdentity other)
        {
            return true;
        }
    }
}