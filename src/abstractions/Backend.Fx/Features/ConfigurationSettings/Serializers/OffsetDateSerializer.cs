using JetBrains.Annotations;
using NodaTime;
using NodaTime.Text;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class OffsetDateSerializer : NodaTimePatternSerializer<OffsetDate>
    {
        public OffsetDateSerializer() : base(OffsetDatePattern.FullRoundtrip) { }
    }
}