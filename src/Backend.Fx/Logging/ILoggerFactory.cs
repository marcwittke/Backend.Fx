namespace Backend.Fx.Logging
{
    public interface ILoggerFactory
    {
        ILogger Create(string s);
        void BeginActivity(int activityIndex);
    }
}
