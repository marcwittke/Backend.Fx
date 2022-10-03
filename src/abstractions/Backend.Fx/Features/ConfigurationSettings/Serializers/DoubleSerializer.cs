using System.Globalization;
using JetBrains.Annotations;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class DoubleSerializer : ISettingSerializer<double?>
    {
        public string Serialize(double? setting)
        {
            return setting?.ToString("r", CultureInfo.InvariantCulture);
        }

        public double? Deserialize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : double.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}