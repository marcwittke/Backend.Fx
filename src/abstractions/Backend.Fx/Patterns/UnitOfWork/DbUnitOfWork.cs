// using System;
// using System.Data;
// using System.Data.Common;
// using System.Security.Principal;
// using Backend.Fx.Environment.DateAndTime;
// using Backend.Fx.Logging;
// using Backend.Fx.Patterns.DependencyInjection;
// using Backend.Fx.Patterns.EventAggregation.Domain;
// using Backend.Fx.Patterns.EventAggregation.Integration;
//
// namespace Backend.Fx.Patterns.UnitOfWork
// {
//     public abstract class DbUnitOfWork : UnitOfWork
//     {
//         private static readonly ILogger Logger = LogManager.Create<DbUnitOfWork>();
//         private IDisposable _transactionLifetimeLogger;
//         private readonly bool _shouldHandleConnectionState;
//
//         protected DbUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder, IDomainEventAggregator eventAggregator,
//             IEventBusScope eventBusScope, DbConnection dbConnection)
//             : base(clock, identityHolder, eventAggregator, eventBusScope)
//         {
//             Connection = dbConnection;
//             if (Connection.State == ConnectionState.Open)
//             {
//                 _shouldHandleConnectionState = false;
//             }
//         }
//         
//         public DbConnection Connection { get; }
//         
//         public DbTransaction CurrentTransaction { get; private set; }
//
//         public override void Begin()
//         {
//             base.Begin();
//             if (_shouldHandleConnectionState) Connection.Open();
//             CurrentTransaction = Connection.BeginTransaction();
//             _transactionLifetimeLogger = Logger.DebugDuration("Transaction open");
//         }
//
//         protected override void Commit()
//         {
//             CurrentTransaction.Commit();
//             CurrentTransaction.Dispose();
//             CurrentTransaction = null;
//             _transactionLifetimeLogger?.Dispose();
//             _transactionLifetimeLogger = null;
//             if (_shouldHandleConnectionState) Connection.Close();
//         }
//
//         protected override void Rollback()
//         {
//             Logger.Info("Rolling back transaction");
//             try
//             {
//                 CurrentTransaction?.Rollback();
//                 CurrentTransaction?.Dispose();
//                 CurrentTransaction = null;
//                 if (_shouldHandleConnectionState && Connection.State == ConnectionState.Open)
//                 {
//                     Connection.Close();
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Logger.Error(ex, "Rollback failed");
//             }
//             _transactionLifetimeLogger?.Dispose();
//             _transactionLifetimeLogger = null;
//         }
//
//         protected override void Dispose(bool disposing)
//         {
//             base.Dispose(disposing);
//             if (disposing)
//             {
//                 _transactionLifetimeLogger?.Dispose();
//                 CurrentTransaction?.Dispose();
//             }
//         }
//     }
// }