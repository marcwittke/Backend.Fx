namespace Backend.Fx.Environment.Authentication
{
    using System.Security.Principal;

    public class SystemIdentity : IIdentity
    {
        public string Name { get { return "SYSTEM"; } }

        public string AuthenticationType { get { return "system internal"; } }

        public bool IsAuthenticated { get { return true; } }
    }
}
