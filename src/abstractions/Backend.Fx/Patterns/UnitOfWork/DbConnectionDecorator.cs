using System.Data;
using System.Security.Principal;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.UnitOfWork
{
    /// <summary>
    /// Enriches the unit of work to open and close a database connection during lifetime
    /// </summary>
    public class DbConnectionDecorator : IUnitOfWork
    {
        public DbConnectionDecorator(IDbConnection dbConnection, IUnitOfWork unitOfWork)
        {
            DbConnection = dbConnection;
            UnitOfWork = unitOfWork;
        }

        public IUnitOfWork UnitOfWork { get; }

        public IDbConnection DbConnection { get; }

        public ICurrentTHolder<IIdentity> IdentityHolder => UnitOfWork.IdentityHolder;

        public void Begin()
        {
            DbConnection.Open();
            UnitOfWork.Begin();
        }

        public void Complete()
        {
            UnitOfWork.Complete();
            DbConnection.Close();
        }
    }
}