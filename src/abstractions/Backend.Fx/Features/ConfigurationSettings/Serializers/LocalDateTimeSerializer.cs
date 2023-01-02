using JetBrains.Annotations;
using NodaTime;
using NodaTime.Text;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class LocalDateTimeSerializer : NodaTimePatternSerializer<LocalDateTime>
    {
        public LocalDateTimeSerializer() : base(LocalDateTimePattern.BclRoundtrip) { }
    }
}