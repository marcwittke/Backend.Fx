using JetBrains.Annotations;
using NodaTime;
using NodaTime.Text;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class InstantSerializer : NodaTimePatternSerializer<Instant>
    {
        public InstantSerializer() : base(InstantPattern.ExtendedIso) { }
    }
}