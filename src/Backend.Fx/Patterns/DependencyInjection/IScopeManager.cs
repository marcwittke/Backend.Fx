namespace Backend.Fx.Patterns.DependencyInjection
{
    using System.Security.Principal;
    using Environment.MultiTenancy;

    public interface IScopeManager
    {
        /// <summary>
        /// Begins a new injection scope for the given identity. The identity may be used 
        /// for authorization purposes inside the domain layer. The caller is responsable to complete
        /// the scope after finalization.
        /// </summary>
        IScope BeginScope(IIdentity identity, TenantId tenantId);
    }
}
