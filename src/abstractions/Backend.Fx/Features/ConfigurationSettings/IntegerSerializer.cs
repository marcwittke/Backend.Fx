using System.Globalization;
using JetBrains.Annotations;

namespace Backend.Fx.Features.ConfigurationSettings
{
    [UsedImplicitly]
    public class IntegerSerializer : ISettingSerializer<int?>
    {
        public string Serialize(int? setting)
        {
            return setting?.ToString(CultureInfo.InvariantCulture);
        }

        public int? Deserialize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (int?) null : int.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}