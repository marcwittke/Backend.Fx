namespace Backend.Fx.Features
{
    /// <summary>
    /// Marks a <see cref="Feature"/> to add behavior in case of multi tenancy enabled
    /// </summary>
    public interface IMultiTenancyFeature
    {
        void EnableMultiTenancyServices(IBackendFxApplication application);
    }
}