using System.Data;
using System.Data.Common;
using Backend.Fx.Patterns.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence
{
    public class EfUnitOfWorkTransactionDecorator<TDbContext> : UnitOfWorkTransactionDecorator where TDbContext : DbContext
    {
        private readonly EfUnitOfWork<TDbContext> _unitOfWork;

        public EfUnitOfWorkTransactionDecorator(IDbConnection dbConnection, EfUnitOfWork<TDbContext> unitOfWork) : base(dbConnection, unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override void Begin()
        {
            base.Begin();
            _unitOfWork.DbContext.Database.UseTransaction((DbTransaction) TransactionContext.CurrentTransaction);
        }
    }
}