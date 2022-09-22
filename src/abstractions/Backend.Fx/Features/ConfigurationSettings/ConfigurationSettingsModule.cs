using Backend.Fx.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.ConfigurationSettings
{
    internal class ConfigurationSettingsModule<TSettingRepository> : IModule 
        where TSettingRepository : class, ISettingRepository
    {
        private readonly SettingSerializerFactory _settingSerializerFactory;

        public ConfigurationSettingsModule(SettingSerializerFactory settingSerializerFactory)
        {
            _settingSerializerFactory = settingSerializerFactory;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            compositionRoot.Register(
                ServiceDescriptor.Singleton<ISettingSerializerFactory>(_settingSerializerFactory));

            compositionRoot.Register(
                ServiceDescriptor.Scoped<ISettingRepository, TSettingRepository>());
        }
    }
}