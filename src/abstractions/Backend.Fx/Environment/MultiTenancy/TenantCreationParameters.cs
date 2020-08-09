namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantCreationParameters
    {
        public TenantCreationParameters()
        {
        }

        public TenantCreationParameters(string name, string description, bool isDemonstrationTenant)
        {
            Name = name;
            Description = description;
            IsDemonstrationTenant = isDemonstrationTenant;
        }

        public bool IsDemonstrationTenant { get; set; }
        public string Name { get; set; } = "Tenant";
        public string Description { get; set; }
    }
}