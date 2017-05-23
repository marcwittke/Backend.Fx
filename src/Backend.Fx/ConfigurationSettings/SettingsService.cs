namespace Backend.Fx.ConfigurationSettings
{
    using System.Linq;
    using BuildingBlocks;

    public abstract class SettingsService
    {
        private readonly IRepository<Setting> settingRepository;
        private static readonly SettingSerializerFactory SettingSerializerFactory = new SettingSerializerFactory();

        protected SettingsService(IRepository<Setting> settingRepository)
        {
            this.settingRepository = settingRepository;
        }

        protected T ReadSetting<T>(string key)
        {
            var setting = settingRepository.AggregateQueryable.SingleOrDefault(s => s.Key == key.ToString());
            if (setting == null)
            {
                return default(T);
            }
            var serializer = SettingSerializerFactory.GetSerializer<T>();
            return setting.GetValue(serializer);
        }

        protected void WriteSetting<T>(string key, T value) 
        {
            var setting = settingRepository.AggregateQueryable.SingleOrDefault(s => s.Key == key.ToString());
            if (setting == null)
            {
                setting =  new Setting(key);
                settingRepository.Add(setting);
            }
            var serializer = SettingSerializerFactory.GetSerializer<T>();
            setting.SetValue(serializer, value);
        }

    }
}