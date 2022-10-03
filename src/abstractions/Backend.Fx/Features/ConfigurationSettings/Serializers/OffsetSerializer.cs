using JetBrains.Annotations;
using NodaTime;
using NodaTime.Text;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class OffsetSerializer : NodaTimePatternSerializer<Offset>
    {
        public OffsetSerializer() : base(OffsetPattern.GeneralInvariant) { }
    }
}