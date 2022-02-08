using System;

namespace Backend.Fx.Logging
{
    [Obsolete]
    public interface ILoggerFactory
    {
        ILogger Create(string s);
        ILogger Create(Type t);
        ILogger Create<T>();
        
        void BeginActivity(int activityIndex);
        void Shutdown();
    }
}
