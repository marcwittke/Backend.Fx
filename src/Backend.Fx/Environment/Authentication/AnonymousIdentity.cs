namespace Backend.Fx.Environment.Authentication
{
    using System.Security.Principal;

    public class AnonymousIdentity : IIdentity
    {
        public string Name { get { return "anonymous"; } }

        public string AuthenticationType { get { return "system internal"; } }

        public bool IsAuthenticated { get { return false; } }
    }
}