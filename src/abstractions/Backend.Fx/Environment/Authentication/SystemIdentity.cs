using System.Security.Principal;

namespace Backend.Fx.Environment.Authentication
{
    public class SystemIdentity : IIdentity
    {
        public string Name => "SYSTEM";

        public string AuthenticationType => "system internal";

        public bool IsAuthenticated => true;
    }
}
