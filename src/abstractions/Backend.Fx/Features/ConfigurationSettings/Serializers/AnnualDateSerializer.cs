using JetBrains.Annotations;
using NodaTime;
using NodaTime.Text;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class AnnualDateSerializer : NodaTimePatternSerializer<AnnualDate>
    {
        public AnnualDateSerializer() : base(AnnualDatePattern.Iso) { }
    }
}