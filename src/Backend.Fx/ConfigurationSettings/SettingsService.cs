namespace Backend.Fx.ConfigurationSettings
{
    using System.Linq;
    using BuildingBlocks;
    using Patterns.IdGeneration;

    public abstract class SettingsService
    {
        private readonly string category;
        private readonly IEntityIdGenerator idGenerator;
        private readonly IRepository<Setting> settingRepository;
        private readonly ISettingSerializerFactory settingSerializerFactory;

        protected SettingsService(string category, IEntityIdGenerator idGenerator, IRepository<Setting> settingRepository, ISettingSerializerFactory settingSerializerFactory)
        {
            this.category = category;
            this.idGenerator = idGenerator;
            this.settingRepository = settingRepository;
            this.settingSerializerFactory = settingSerializerFactory;
        }

        protected T ReadSetting<T>(string key)
        {
            var setting = settingRepository.AggregateQueryable.SingleOrDefault(s => s.Key == category + "." + key);
            if (setting == null)
            {
                return default(T);
            }
            var serializer = settingSerializerFactory.GetSerializer<T>();
            return setting.GetValue(serializer);
        }

        protected void WriteSetting<T>(string key, T value)
        {
            var setting = settingRepository.AggregateQueryable.SingleOrDefault(s => s.Key == category + "." + key);
            if (setting == null)
            {
                setting = new Setting(idGenerator.NextId(), category + "." + key);
                settingRepository.Add(setting);
            }
            var serializer = settingSerializerFactory.GetSerializer<T>();
            setting.SetValue(serializer, value);
        }
    }
}