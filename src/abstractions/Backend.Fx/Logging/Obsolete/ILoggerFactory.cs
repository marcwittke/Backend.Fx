using System;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace
namespace Backend.Fx.Logging
{
    [Obsolete, PublicAPI]
    public interface ILoggerFactory
    {
        ILogger Create(string s);
        ILogger Create(Type t);
        ILogger Create<T>();
        
        void BeginActivity(int activityIndex);
        void Shutdown();
    }
}
