using System.Globalization;
using JetBrains.Annotations;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class ShortSerializer : ISettingSerializer<short?>
    {
        public string Serialize(short? setting)
        {
            return setting?.ToString(CultureInfo.InvariantCulture);
        }

        public short? Deserialize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : short.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}