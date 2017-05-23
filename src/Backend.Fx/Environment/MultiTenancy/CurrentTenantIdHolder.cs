namespace Backend.Fx.Environment.MultiTenancy
{
    using Patterns.DependencyInjection;

    public class CurrentTenantIdHolder : CurrentTHolder<TenantId>
    {
        public override TenantId ProvideInitialInstance()
        {
            return new TenantId(null);
        }

        protected override string Describe(TenantId instance)
        {
            if (instance == null)
            {
                return "<NULL>";
            }

            if (instance.HasValue)
            {
                return $"TenantId: {instance.Value}";
            }
            
            return "TenantId: null";
        }
    }
}