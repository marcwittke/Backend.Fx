using System.Globalization;
using JetBrains.Annotations;

namespace Backend.Fx.Features.ConfigurationSettings
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
            return string.IsNullOrWhiteSpace(value) ? (double?) null : double.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}