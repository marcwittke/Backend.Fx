namespace Backend.Fx.Environment.Authentication
{
    using System.Security.Principal;

    public class SystemIdentity : IIdentity
    {
        public string Name => "SYSTEM";

        public string AuthenticationType => "system internal";

        public bool IsAuthenticated => true;
    }
}
