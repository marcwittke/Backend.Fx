using System.Globalization;
using JetBrains.Annotations;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class DecimalSerializer : ISettingSerializer<decimal?>
    {
        public string Serialize(decimal? setting)
        {
            return setting?.ToString("G", CultureInfo.InvariantCulture);
        }

        public decimal? Deserialize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : decimal.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}