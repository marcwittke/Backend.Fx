using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Fx.AspNetCore.Mvc.DependencyInjection
{
    public class BackendFxApplicationHubActivator<T> : BackendFxApplicationActivator, IHubActivator<T> where T : Hub
    {
        public BackendFxApplicationHubActivator(IBackendFxApplication application) : base(application)
        {
        }


        public T Create()
        {
            return (T)GetInstance(typeof(T));
        }

        public void Release(T hub)
        {
        }
    }
}