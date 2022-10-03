using JetBrains.Annotations;

namespace Backend.Fx.Features.ConfigurationSettings.Serializers
{
    [UsedImplicitly]
    public class StringSerializer : ISettingSerializer<string>
    {
        public string Serialize(string setting)
        {
            return setting;
        }

        public string Deserialize(string value)
        {
            return value;
        }
    }
}