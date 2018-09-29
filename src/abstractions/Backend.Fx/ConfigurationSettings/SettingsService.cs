namespace Backend.Fx.ConfigurationSettings
{
    using System.Linq;
    using BuildingBlocks;
    using Patterns.IdGeneration;

    public abstract class SettingsService
    {
        private readonly string _category;
        private readonly IEntityIdGenerator _idGenerator;
        private readonly IRepository<Setting> _settingRepository;
        private readonly ISettingSerializerFactory _settingSerializerFactory;

        protected SettingsService(string category, IEntityIdGenerator idGenerator, IRepository<Setting> settingRepository, ISettingSerializerFactory settingSerializerFactory)
        {
            _category = category;
            _idGenerator = idGenerator;
            _settingRepository = settingRepository;
            _settingSerializerFactory = settingSerializerFactory;
        }

        protected T ReadSetting<T>(string key)
        {
            var setting = _settingRepository.AggregateQueryable.SingleOrDefault(s => s.Key == _category + "." + key);
            if (setting == null)
            {
                return default(T);
            }
            var serializer = _settingSerializerFactory.GetSerializer<T>();
            return setting.GetValue(serializer);
        }

        protected void WriteSetting<T>(string key, T value)
        {
            var setting = _settingRepository.AggregateQueryable.SingleOrDefault(s => s.Key == _category + "." + key);
            if (setting == null)
            {
                setting = new Setting(_idGenerator.NextId(), _category + "." + key);
                _settingRepository.Add(setting);
            }
            var serializer = _settingSerializerFactory.GetSerializer<T>();
            setting.SetValue(serializer, value);
        }
    }
}