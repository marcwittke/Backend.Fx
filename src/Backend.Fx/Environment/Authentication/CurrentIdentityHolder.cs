namespace Backend.Fx.Environment.Authentication
{
    using System.Security.Principal;
    using Patterns.DependencyInjection;

    public class CurrentIdentityHolder : CurrentTHolder<IIdentity>
    {
        public override IIdentity ProvideInstance()
        {
            return new AnonymousIdentity();
        }

        protected override string Describe(IIdentity instance)
        {
            if (instance == null)
            {
                return "<NULL>";
            }

            string auth = instance.IsAuthenticated ? $"authenticated via {instance.AuthenticationType}" : "not authenticated";
            return $"Identity: {instance.Name}, {auth}";
        }
    }
}
