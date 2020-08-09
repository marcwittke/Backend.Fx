using System.Security.Principal;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence
{
    public class DbSession : ICanFlush
    {
        private readonly DbContext _dbContext;
        private readonly ICurrentTHolder<IIdentity> _identityHolder;
        private readonly IClock _clock;

        public DbSession(DbContext dbContext, ICurrentTHolder<IIdentity> identityHolder, IClock clock)
        {
            _dbContext = dbContext;
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            _identityHolder = identityHolder;
            _clock = clock;
        }
        
        public void Flush()
        {
            _dbContext.ChangeTracker.DetectChanges();
            _dbContext.UpdateTrackingProperties(_identityHolder.Current.Name, _clock.UtcNow);
            _dbContext.SaveChanges();
        }
    }
}