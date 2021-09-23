using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class CurrentTenantIdHolder : CurrentTHolder<TenantId>
    {
        public CurrentTenantIdHolder()
        { }

        private CurrentTenantIdHolder(TenantId initial) : base(initial)
        { }

        public static CurrentTenantIdHolder Create(int tenantId)
        {
            var instance = new CurrentTenantIdHolder((TenantId)tenantId);
            return instance;
        }

        public static CurrentTenantIdHolder Create(TenantId tenantId)
        {
            var instance = new CurrentTenantIdHolder(tenantId);
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
