using JetBrains.Annotations;
using NodaTime;
using NodaTime.Text;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class OffsetTimeSerializer : NodaTimePatternSerializer<OffsetTime>
    {
        public OffsetTimeSerializer() : base(OffsetTimePattern.ExtendedIso) { }
    }
}