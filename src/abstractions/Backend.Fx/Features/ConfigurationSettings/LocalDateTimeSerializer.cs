using System;
using System.Globalization;
using JetBrains.Annotations;
using NodaTime;

namespace Backend.Fx.Features.ConfigurationSettings
{
    [UsedImplicitly]
    public class LocalDateTimeSerializer : ISettingSerializer<LocalDateTime?>
    {
        public string Serialize(LocalDateTime? setting)
        {
            return setting?.ToString("O", CultureInfo.InvariantCulture);
        }

        public LocalDateTime? Deserialize(string value)
        {
            return string.IsNullOrWhiteSpace(value) 
                ? (LocalDateTime?) null 
                : LocalDateTime.FromDateTime(DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        }
    }
}