using System.Collections.Concurrent;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantEvents
    {
        private readonly ConcurrentQueue<TenantEvent> _events = new ConcurrentQueue<TenantEvent>();

        public void Publish(TenantEvent tenantEvent)
        {
            _events.Enqueue(tenantEvent);
        }
    }
}