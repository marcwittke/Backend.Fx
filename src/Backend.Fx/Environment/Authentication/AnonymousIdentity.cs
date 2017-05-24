namespace Backend.Fx.Environment.Authentication
{
    using System.Security.Principal;

    public class AnonymousIdentity : IIdentity
    {
        public string Name { get { return "ANONYMOUS"; } }

        public string AuthenticationType { get { return string.Empty; } }

        public bool IsAuthenticated { get { return false; } }
    }
}