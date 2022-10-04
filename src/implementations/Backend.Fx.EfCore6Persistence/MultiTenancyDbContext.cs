using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Domain;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Backend.Fx.EfCore6Persistence;

public abstract class MultiTenancyDbContext : DbContext
{
    private readonly ICurrentTHolder<TenantId> _tenantIdHolder;

    private int? TenantIdForGlobalQueryFilter =>
        _tenantIdHolder.Current.HasValue ? _tenantIdHolder.Current.Value : null;

    protected MultiTenancyDbContext(ICurrentTHolder<TenantId> tenantIdHolder, DbContextOptions options)
        : base(options)
    {
        _tenantIdHolder = tenantIdHolder;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        IMutableEntityType[] aggregateRootEntityTypes = modelBuilder
            .Model
            .GetEntityTypes()
            .Where(met =>
                met.ClrType.IsClass
                && !met.ClrType.IsAbstract
                && typeof(IAggregateRoot).IsAssignableFrom(met.ClrType))
            .ToArray();

        foreach (IMutableEntityType entityType in aggregateRootEntityTypes)
        {
            // add a shadow property for the tenant id
            modelBuilder.Entity(entityType.ClrType).Property<int>("TenantId");

            // add a global query filter expression that is equivalent to:
            // e => e.TenantId == this.TenantIdForGlobalQueryFilter
            ParameterExpression parameter = Expression.Parameter(entityType.ClrType, "e");
            BinaryExpression body = Expression.Equal(
                Expression.Call(
                    typeof(EF),
                    nameof(EF.Property),
                    new[] { typeof(int?) },
                    parameter,
                    Expression.Constant("TenantId")),
                Expression.Property(
                    Expression.Constant(this),
                    typeof(MultiTenancyDbContext).GetProperty(
                        nameof(TenantIdForGlobalQueryFilter),
                        BindingFlags.Instance | BindingFlags.NonPublic)!));

            LambdaExpression queryFilterExpression = Expression.Lambda(body, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(queryFilterExpression);
        }
    }

    public override int SaveChanges()
    {
        ApplyTenantIdToAddedEntities();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyTenantIdToAddedEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantIdToAddedEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyTenantIdToAddedEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyTenantIdToAddedEntities()
    {
        if (!_tenantIdHolder.Current.HasValue)
        {
            throw new InvalidOperationException(
                "Attempt to call SaveChanges() with added entities but with no tenant id available");
        }
        
        ChangeTracker
            .Entries()
            .Where(entry => entry.Entity is IAggregateRoot)
            .Where(entry => entry.State == EntityState.Added)
            .AsParallel()
            .ForAll(entry => entry.Property("TenantId").CurrentValue = _tenantIdHolder.Current.Value);
    }
}