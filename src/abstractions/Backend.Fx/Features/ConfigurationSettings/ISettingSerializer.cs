namespace Backend.Fx.Features.ConfigurationSettings
{
    public interface ISettingSerializer
    {
    }

    public interface ISettingSerializer<T> : ISettingSerializer
    {
        string Serialize(T setting);
        T Deserialize(string value);
    }
}