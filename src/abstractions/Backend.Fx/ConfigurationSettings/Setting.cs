using Backend.Fx.BuildingBlocks;
using JetBrains.Annotations;

namespace Backend.Fx.ConfigurationSettings
{
    public class Setting : AggregateRoot
    {
        [UsedImplicitly]
        private Setting()
        { }

        public Setting(int id, string key) : base(id)
        {
            Key = key;
        }

        public string Key { get; [UsedImplicitly] private set; }

        public string SerializedValue { get; private set; }

        public T GetValue<T>(ISettingSerializer<T> serializer)
        {
            return serializer.Deserialize(SerializedValue);
        }

        public void SetValue<T>(ISettingSerializer<T> serializer, T value)
        {
            SerializedValue = serializer.Serialize(value);
        }
    }
}
