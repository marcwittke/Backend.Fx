using JetBrains.Annotations;

namespace Backend.Fx.Features.ConfigurationSettings
{
    [PublicAPI]
    public interface ISettingRepository
    {
        /// <summary>
        /// Gets the serialized string value for a specific setting key in a specific category
        /// </summary>
        /// <param name="category">The category of the setting</param>
        /// <param name="key">The key of the setting</param>
        /// <returns>The serialized value of the configuration setting, or <c>null</c> when not configured.</returns>
        [CanBeNull]
        string GetSerializedValue([NotNull] string category, [NotNull] string key);

        /// <summary>
        /// Writes the serialized string value for a specific setting key in a specific category
        /// </summary>
        /// <param name="category">The category of the setting</param>
        /// <param name="key">The key of the setting</param>
        /// <param name="serializedValue">The serialized value of the configuration setting</param>
        void WriteSerializedValue([NotNull] string category, [NotNull] string key, [CanBeNull] string serializedValue);
    }
}