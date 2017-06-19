namespace Backend.Fx.ConfigurationSettings
{
    using BuildingBlocks;
    using JetBrains.Annotations;

    public class Setting : AggregateRoot
    {
        [UsedImplicitly]
        private Setting() { }

        public Setting(int id, string key)
        {
            Id = id;
            Key = key;
        }

        public string Key { get; private set; }
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