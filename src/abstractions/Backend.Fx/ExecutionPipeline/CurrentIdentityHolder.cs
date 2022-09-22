using System.Security.Principal;
using Backend.Fx.Util;
using JetBrains.Annotations;

namespace Backend.Fx.ExecutionPipeline
{
    [PublicAPI]
    public sealed class CurrentIdentityHolder : CurrentTHolder<IIdentity>
    {
        public CurrentIdentityHolder()
        { }

        private CurrentIdentityHolder(IIdentity initial) : base(initial)
        { }
        
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
            return new CurrentIdentityHolder(identity);
        }
    }
}