using Backend.Fx.Exceptions;
using Backend.Fx.Features.Persistence;
using Backend.Fx.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.EfCore6Persistence
{
    public class EfFlush : ICanFlush
    {
        private readonly ICanFlush _canFlush;
        private readonly ILogger _logger = Log.Create<EfFlush>();
        
        public DbContext DbContext { get; }

        public EfFlush(DbContext dbContext, ICanFlush canFlush)
        {
            _canFlush = canFlush;
            DbContext = dbContext;
            _logger.LogInformation("Disabling change tracking on {DbContextTypeName} instance", dbContext.GetType().Name);
            DbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        }

        public void Flush()
        {
            DetectChanges();
            SaveChanges();
            _canFlush.Flush();
        }

        private void DetectChanges()
        {
            using (_logger.LogDebugDuration("Detecting changes"))
            {
                DbContext.ChangeTracker.DetectChanges();
            }
        }

        
        private void SaveChanges()
        {
            using (_logger.LogDebugDuration("Saving changes"))
            {
                try
                {
                    DbContext.SaveChanges();
                }
                catch (DbUpdateConcurrencyException concurrencyException)
                {
                    throw new ConflictedException("Saving changes failed due to optimistic concurrency violation", concurrencyException);
                }
            }
        }
        
    }
}