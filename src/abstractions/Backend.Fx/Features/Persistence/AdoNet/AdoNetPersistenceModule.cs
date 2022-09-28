using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.Persistence.AdoNet
{
    public abstract class AdoNetPersistenceModule : PersistenceModule
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        protected AdoNetPersistenceModule(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        protected override void RegisterInfrastructure(ICompositionRoot compositionRoot)
        {
            // the DbConnectionFactory is registered as a singleton
            compositionRoot.Register(ServiceDescriptor.Singleton(_dbConnectionFactory));

            // by letting the container create the connection we can be sure, that only one connection per scope is used, and disposing is done accordingly
            compositionRoot.Register(ServiceDescriptor.Scoped(_ => _dbConnectionFactory.Create()));

            // wrapping the operation:
            //   invoke   -> connection.open  -> transaction.begin ---+
            //                                                        |
            //                                                        v
            //                                                      operation
            //                                                        |
            //                                                        v
            //                                                      flush
            //                                                        |
            // end invoke <- connection.close <- transaction.commit <-+ 
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, DbTransactionOperationDecorator>());
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, DbConnectionOperationDecorator>());
        }
    }
}