using JetBrains.Annotations;

namespace Backend.Fx.Features.ConfigurationSettings
{
    /// <summary>
    /// The feature "Configuration Settings" provides a simple abstraction over an arbitrary key/value configuration
    /// setting store. The default <see cref="SettingSerializerFactory"/> already provides serialization to and from
    /// string for various configuration setting types, but you can provide your own implementation to extend the
    /// functionality.
    /// </summary>
    /// <typeparam name="TSettingRepository">The abstraction over your key/value store. Instances of this type will
    /// be injected with a scoped lifetime.</typeparam>
    [PublicAPI]
    public class ConfigurationSettingsFeature<TSettingRepository> : Feature
        where TSettingRepository : class, ISettingRepository
    {
        private readonly SettingSerializerFactory _settingSerializerFactory;

        /// <param name="settingSerializerFactory">The factory that provides serializers. A singleton instance is being held.</param>
        public ConfigurationSettingsFeature(SettingSerializerFactory settingSerializerFactory = null)
        {
            _settingSerializerFactory = settingSerializerFactory ?? new SettingSerializerFactory();
        }

        public override void Enable(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(
                new ConfigurationSettingsModule<TSettingRepository>(_settingSerializerFactory, application.Assemblies));
        }
    }
}