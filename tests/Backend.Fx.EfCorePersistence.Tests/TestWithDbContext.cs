using System;
using System.Collections.Generic;
using System.Data;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Tests
{
    public abstract class TestWithDbContext : IDisposable
    {
        public IDbConnection Connection { get; }

        public ICurrentTHolder<TenantId> TenantIdHolder { get; } = new CurrentTenantIdHolder();
        public IClock Clock { get; } = new FrozenClock();

        protected TestWithDbContext(IDbConnection dbConnection)
        {
            TenantIdHolder.ReplaceCurrent(new TenantId(12));
            Connection = dbConnection;
        }

        protected void CreateDatabase()
        {
            using (var dbContext = new TestDbContext(DbContextOptions()))
            {
                dbContext.Database.EnsureCreated();
            }
        }

        protected void ExecuteNonQuery(string cmd)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = cmd;
                command.ExecuteNonQuery();
            }
        }

        protected T ExecuteScalar<T>(string cmd)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = cmd;
                return (T)command.ExecuteScalar();
            }
        }

        [UsedImplicitly]
        protected IEnumerable<T> ExecuteScalar<T>(string cmd, Func<IDataReader, T> forEachResultFunc)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = cmd;
                IDataReader reader = command.ExecuteReader();
                while (reader.NextResult())
                {
                    yield return forEachResultFunc(reader);
                }
            }
        }

        public void Dispose()
        {
            Connection?.Close();
            Connection?.Dispose();
        }

        public abstract DbContextOptions<TestDbContext> DbContextOptions();
        public abstract TestDbContext UseDbContext();
    }
}