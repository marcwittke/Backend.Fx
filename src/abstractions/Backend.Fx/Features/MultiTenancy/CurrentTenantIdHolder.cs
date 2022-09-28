using Backend.Fx.Util;
using JetBrains.Annotations;

namespace Backend.Fx.Features.MultiTenancy
{
    public class CurrentTenantIdHolder : CurrentTHolder<TenantId>
    {
        [UsedImplicitly]
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
            return instance.HasValue ? $"TenantId: {instance.Value}" : "TenantId: null";
        }
    }
}