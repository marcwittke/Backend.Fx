using System;
using Backend.Fx.Features.MultiTenancy;
using JetBrains.Annotations;

namespace Backend.Fx.Features.MultiTenancyAdmin
{
    /// <summary>
    /// Represents a tenant in the application
    /// </summary>
    public class Tenant
    {
        [UsedImplicitly]
        private Tenant()
        {
        }

        public Tenant(int id, [NotNull] string name, string description, bool isDemoTenant, string configuration = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            Id = id;
            Name = name;
            Description = description;
            IsDemoTenant = isDemoTenant;
            Configuration = configuration;
            IsActive = true;
        }

        public int Id { get; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsDemoTenant { get; }

        public bool IsActive { get; set; }

        /// <summary>
        /// optional: a generic field to store your arbitrary config data 
        /// </summary>
        public string Configuration { get; set; }

        public TenantId TenantId => new(Id);
    }
}