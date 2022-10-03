using JetBrains.Annotations;
using NodaTime;
using NodaTime.Text;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class LocalDateSerializer : NodaTimePatternSerializer<LocalDate>
    {
        public LocalDateSerializer() : base(LocalDatePattern.FullRoundtrip) { }
    }
}