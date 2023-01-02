using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class DateTimeOffsetSerializer : ISettingSerializer<DateTimeOffset?>
    {
        public string Serialize(DateTimeOffset? setting)
        {
            return setting?.ToString("O", CultureInfo.InvariantCulture);
        }

        public DateTimeOffset? Deserialize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }
    }
}