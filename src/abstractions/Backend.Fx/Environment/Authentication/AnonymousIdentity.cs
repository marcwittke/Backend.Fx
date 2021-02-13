using System.Security.Principal;

namespace Backend.Fx.Environment.Authentication
{
    public class AnonymousIdentity : IIdentity
    {
        public string Name => "ANONYMOUS";

        public string AuthenticationType => string.Empty;

        public bool IsAuthenticated => false;
    }
}