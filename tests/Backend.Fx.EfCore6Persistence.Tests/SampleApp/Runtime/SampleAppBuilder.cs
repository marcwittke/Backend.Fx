using System.Data.Common;
using Backend.Fx.EfCore6Persistence.Bootstrapping;
using Backend.Fx.EfCore6Persistence.Tests.SampleApp.Persistence;
using Backend.Fx.Features.Persistence;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SampleApp.Domain;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Backend.Fx.EfCore6Persistence.Tests.SampleApp.Runtime
{
    public static class SampleAppBuilder
    {
        public static IBackendFxApplication Build(CompositionRootType compositionRootType, string connectionString)
        {
            var dbConnectionFactory = A.Fake<IDbConnectionFactory>();
            A.CallTo(() => dbConnectionFactory.Create()).Returns(new SqliteConnection(connectionString));

            IBackendFxApplication application = new BackendFxApplication(
                compositionRootType.Create(),
                A.Fake<IExceptionLogger>(),
                typeof(BlogMapping).Assembly,
                typeof(Blog).Assembly);

            application = new PersistentApplication(
                new SampleAppDbBootstrapper(connectionString),
                A.Fake<IDatabaseAvailabilityAwaiter>(),
                new EfCorePersistenceModule<SampleAppDbContext, SampleAppIdGenerator>(
                    dbConnectionFactory,
                    A.Fake<ILoggerFactory>(),
                    (builder, connection) => builder.UseSqlite((DbConnection)connection),
                    false,
                    application.Assemblies
                ),
                application);

            return application;
        }
    }
}