namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantCreationParameters
    {
        public TenantCreationParameters()
        {
        }

        public TenantCreationParameters(string name, string description, bool isDemonstrationTenant, string defaultCultureName=null)
        {
            Name = name;
            Description = description;
            IsDemonstrationTenant = isDemonstrationTenant;
            DefaultCultureName = defaultCultureName;
        }

        public bool IsDemonstrationTenant { get; set; } = false;
        public string Name { get; set; } = "Tenant";
        public string DefaultCultureName { get; set; } = "en-US";
        public string Description { get; set; }
    }
}