using System.Security.Principal;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Environment.Persistence
{
    public interface IPersistenceSession : ICanFlush
    {
        ICurrentTHolder<IIdentity> IdentityHolder { get; }
        
        IClock Clock { get; }

        void MakeReadonly();
    }
}