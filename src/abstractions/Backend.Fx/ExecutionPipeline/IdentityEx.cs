using System.Security.Principal;

namespace Backend.Fx.ExecutionPipeline
{
    public static class IdentityEx
    {
        public static bool IsAnonymous(this IIdentity identity)
        {
            return identity is AnonymousIdentity;
        }
        
        public static bool IsSystem(this IIdentity identity)
        {
            return identity is SystemIdentity;
        }
    }
}