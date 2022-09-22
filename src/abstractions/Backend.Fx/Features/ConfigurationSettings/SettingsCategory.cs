using JetBrains.Annotations;

namespace Backend.Fx.Features.ConfigurationSettings
{
    /// <summary>
    /// Base class to implement a category of settings.
    /// A typical setting would be implemented as a read/write property.
    /// <example>
    /// <code>
    /// public int MyIntegerSetting
    /// {
    ///     get => ReadSetting&lt;int?&gt;(nameof(MyIntegerSetting)) ?? 0;
    ///     set => WriteSetting&lt;int&gt;>(nameof(MyIntegerSetting), value);
    /// }
    /// </code>
    /// </example>
    /// </summary>
    [PublicAPI]
    public abstract class SettingsCategory
    {
        private readonly string _category;
        private readonly ISettingRepository _settingRepository;
        private readonly ISettingSerializerFactory _settingSerializerFactory;

        protected SettingsCategory(string category, ISettingRepository settingRepository, ISettingSerializerFactory settingSerializerFactory)
        {
            _category = category;
            _settingRepository = settingRepository;
            _settingSerializerFactory = settingSerializerFactory;
        }

        protected T ReadSetting<T>(string key)
        {
            var serializedValue = _settingRepository.GetSerializedValue(_category, key);
            if (serializedValue == null)
            {
                return default;
            }

            var serializer = _settingSerializerFactory.GetSerializer<T>();
            return serializer.Deserialize(serializedValue);
        }

        protected void WriteSetting<T>(string key, T value)
        {
            var serializer = _settingSerializerFactory.GetSerializer<T>();
            var serializedValue = serializer.Serialize(value);
            _settingRepository.WriteSerializedValue(_category, key, serializedValue);
        }
    }
}