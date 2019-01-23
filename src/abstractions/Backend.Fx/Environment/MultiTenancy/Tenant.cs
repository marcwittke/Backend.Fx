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

        public Tenant(int id, string name, string description, bool isDemoTenant, TenantState state, bool isDefault, string defaultCultureName, string uriMatchingExpression)
        {
            Id = id;
            Name = name;
            Description = description;
            IsDemoTenant = isDemoTenant;
            State = state;
            IsDefault = isDefault;
            DefaultCultureName = defaultCultureName;
            UriMatchingExpression = uriMatchingExpression;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsDemoTenant { get; set; }
        
        public TenantState State { get; set; }

        public bool IsDefault { get; set; }

        public string DefaultCultureName { get; set; }

        public string UriMatchingExpression { get; set; }

        [NotMapped]
        public CultureInfo DefaultCulture
        {
            get => new CultureInfo(DefaultCultureName);
            set => DefaultCultureName = value.Name;
        }
    }

    public enum TenantState
    {
        Created = 0,
        Initializing = 1,
        Active = 2,
        Inactive = int.MaxValue
    }
}
