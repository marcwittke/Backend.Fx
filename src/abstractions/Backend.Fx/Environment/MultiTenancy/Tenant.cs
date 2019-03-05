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
        [UsedImplicitly]
        private Tenant()
        { }

        public Tenant([NotNull] string name, string description, bool isDemoTenant, string defaultCultureName)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            Name = name;
            Description = description;
            IsDemoTenant = isDemoTenant;
            DefaultCultureName = defaultCultureName;
            State = TenantState.Created;
        }
        
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsDemoTenant { get; set; }
        
        public TenantState State { get; set; }

        public string DefaultCultureName { get; set; }

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
        Active = 2,
        Inactive = -1
    }
}
