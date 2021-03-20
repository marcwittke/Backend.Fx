using System.Security.Principal;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Environment.Authentication
{
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

        public static ICurrentTHolder<IIdentity> CreateSystem()
        {
            return Create(new SystemIdentity());
        }
        
        public static ICurrentTHolder<IIdentity> Create(IIdentity identity)
        {
            var currentIdentityHolder = new CurrentIdentityHolder();
            currentIdentityHolder.ReplaceCurrent(identity);
            return currentIdentityHolder;
        }
    }
}