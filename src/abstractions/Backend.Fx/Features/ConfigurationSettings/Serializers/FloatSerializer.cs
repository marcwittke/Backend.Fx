using System.Globalization;
using JetBrains.Annotations;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class FloatSerializer : ISettingSerializer<float?>
    {
        public string Serialize(float? setting)
        {
            return setting?.ToString("r", CultureInfo.InvariantCulture);
        }

        public float? Deserialize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : float.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}