namespace Backend.Fx.Environment.Authentication
{
    using System.Security.Principal;

    public class AnonymousIdentity : IIdentity
    {
        public string Name => "ANONYMOUS";

        public string AuthenticationType => string.Empty;

        public bool IsAuthenticated => false;
    }
}