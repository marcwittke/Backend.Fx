using JetBrains.Annotations;
using NodaTime;
using NodaTime.Text;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class LocalTimeSerializer : NodaTimePatternSerializer<LocalTime>
    {
        public LocalTimeSerializer() : base(LocalTimePattern.ExtendedIso) { }
    }
}