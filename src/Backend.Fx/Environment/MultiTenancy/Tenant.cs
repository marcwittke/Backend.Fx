namespace Backend.Fx.Environment.MultiTenancy
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Globalization;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents a tenant in the application
    /// </summary>
    public class Tenant
    {
        public const string DemonstrationTenantName = "Demonstration";
        public const string DemonstrationTenantDescription = "Default Demonstration Tenant";

        [UsedImplicitly]
        private Tenant()
        { }

        public Tenant([NotNull] string name, string description, bool isDemoTenant, CultureInfo defaultCulture)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            Name = name;
            Description = description;
            IsDemoTenant = isDemoTenant;
            DefaultCulture = defaultCulture;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsDemoTenant { get; set; }
        
        public bool IsInitialized { get; set; }

        public bool IsActive { get; set; }

        public bool IsDefault { get; set; }

        public string DefaultCultureName { get; private set; }

        [NotMapped]
        public CultureInfo DefaultCulture
        {
            get { return new CultureInfo(DefaultCultureName); }
            set { DefaultCultureName = value.Name; }
        }
    }
}
