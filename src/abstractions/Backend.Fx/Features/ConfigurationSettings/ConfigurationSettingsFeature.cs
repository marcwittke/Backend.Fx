using JetBrains.Annotations;

namespace Backend.Fx.Features.ConfigurationSettings
{
    public static class ConfigurationSettingsFeature
    {
        /// <summary>
        /// The feature "Configuration Settings" provides a simple abstraction over an arbitrary key/value configuration
        /// setting store. The default <see cref="SettingSerializerFactory"/> already provides serialization to and from
        /// string for various configuration setting types, but you can provide your own implementation to extend the
        /// functionality.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="settingSerializerFactory">The factory that provides serializers. A singleton instance is being held.</param>
        /// <typeparam name="TSettingRepository">The abstraction over your key/value store. Instances of this type will
        /// be injected with a scoped lifetime.</typeparam>
        [PublicAPI]
        public static void AddConfigurationSettings<TSettingRepository>(
            this IBackendFxApplication application,
            SettingSerializerFactory settingSerializerFactory = null)
            where TSettingRepository : class, ISettingRepository
        {
            settingSerializerFactory = settingSerializerFactory ?? new SettingSerializerFactory();
            application.CompositionRoot.RegisterModules(
                new ConfigurationSettingsModule<TSettingRepository>(settingSerializerFactory));
        }
    }
}