using System.Data;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Environment.Persistence
{
    public class DbConnectionOperationDecorator : IOperation
    {
        public DbConnectionOperationDecorator(IDbConnection dbConnection, IOperation operation)
        {
            DbConnection = dbConnection;
            Operation = operation;
        }

        public IOperation Operation { get; }

        public IDbConnection DbConnection { get; }

        public void Begin()
        {
            DbConnection.Open();
            Operation.Begin();
        }

        public void Complete()
        {
            Operation.Complete();
            DbConnection.Close();
        }

        public void Cancel()
        {
            Operation.Cancel();
        }
    }
}