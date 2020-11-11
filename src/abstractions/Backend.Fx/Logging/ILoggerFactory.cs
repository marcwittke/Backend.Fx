using System;

namespace Backend.Fx.Logging
{
    public interface ILoggerFactory
    {
        [Obsolete]
        ILogger Create(string s);
        ILogger Create(Type t);
        ILogger Create<T>();
        
        void BeginActivity(int activityIndex);
        void Shutdown();
    }
}
