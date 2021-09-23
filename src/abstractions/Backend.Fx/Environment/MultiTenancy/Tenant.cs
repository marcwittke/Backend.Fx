using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Backend.Fx.Environment.MultiTenancy
{
    /// <summary>
    /// Represents a tenant in the application
    /// </summary>
    public class Tenant
    {
        [UsedImplicitly]
        private Tenant()
        { }

        public Tenant([NotNull] string name, string description, bool isDemoTenant, string configuration = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            Name = name;
            Description = description;
            IsDemoTenant = isDemoTenant;
            Configuration = configuration;
            State = TenantState.Active;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsDemoTenant { get; set; }

        public TenantState State { get; set; }

        /// <summary>
        /// optional: a generic field to store your arbitrary config data
        /// </summary>
        public string Configuration { get; set; }

        public TenantId GetTenantId()
        {
            return new TenantId(Id);
        }
    }


    public enum TenantState
    {
        Active = 2,
        Inactive = -1
    }
}
