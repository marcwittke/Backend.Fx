namespace Backend.Fx.AspNetCore.Mvc
{
    public static class HttpContextItemKey
    {
        public const string TenantId = nameof(TenantId);
        public const string InstanceProvider = nameof(InstanceProvider);
    }
}