using System.Security.Principal;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.InMemoryPersistence
{
    public class InMemoryPersistenceSession : IPersistenceSession
    {
        public InMemoryPersistenceSession(ICurrentTHolder<IIdentity> identityHolder, AdjustableClock clock)
        {
            IdentityHolder = identityHolder;
            Clock = clock;
        }

        public void Flush()
        { }

        public ICurrentTHolder<IIdentity> IdentityHolder { get; }
        
        public IClock Clock { get; }
        
        public void MakeReadonly()
        { }
    }
}