namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantCreationParameters
    {
        public bool IsDemonstrationTenant { get; set; } = false;
        public string Name { get; set; } = "Tenant";
        public string DefaultCultureName { get; set; } = "en-US";
        public string Description { get; set; }
    }
}