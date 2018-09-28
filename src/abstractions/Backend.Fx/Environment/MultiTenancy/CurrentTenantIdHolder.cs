namespace Backend.Fx.Environment.MultiTenancy
{
    using Patterns.DependencyInjection;

    public class CurrentTenantIdHolder : CurrentTHolder<TenantId>
    {
        public static CurrentTenantIdHolder Create(int tenantId)
        {
            var instance = new CurrentTenantIdHolder();
            instance.ReplaceCurrent(new TenantId(tenantId));
            return instance;
        }

        public static CurrentTenantIdHolder Create(TenantId tenantId)
        {
            var instance = new CurrentTenantIdHolder();
            instance.ReplaceCurrent(tenantId);
            return instance;
        }

        public override TenantId ProvideInstance()
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