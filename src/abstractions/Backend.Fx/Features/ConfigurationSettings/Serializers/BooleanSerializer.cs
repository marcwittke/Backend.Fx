using JetBrains.Annotations;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class BooleanSerializer : ISettingSerializer<bool?>
    {
        public string Serialize(bool? setting)
        {
            return setting?.ToString();
        }

        public bool? Deserialize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (bool?) null : bool.Parse(value);
        }
    }
}