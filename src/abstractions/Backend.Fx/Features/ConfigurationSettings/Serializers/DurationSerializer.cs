using JetBrains.Annotations;
using NodaTime;
using NodaTime.Text;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class DurationSerializer : NodaTimePatternSerializer<Duration>
    {
        public DurationSerializer() : base(DurationPattern.Roundtrip) { }
    }
}