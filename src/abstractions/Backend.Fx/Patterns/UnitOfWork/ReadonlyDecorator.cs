using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.UnitOfWork
{
    public class ReadonlyDecorator : IUnitOfWork
    {
        private readonly IUnitOfWork _unitOfWorkImplementation;

        public ReadonlyDecorator(IUnitOfWork unitOfWorkImplementation)
        {
            _unitOfWorkImplementation = unitOfWorkImplementation;
        }

        public void Dispose()
        {
            _unitOfWorkImplementation.Dispose();
        }

        public void Begin()
        {
            _unitOfWorkImplementation.Begin();
        }

        public Task CompleteAsync()
        {
            // prevent completion, results in rollback on disposal
            return Task.CompletedTask;
        }

        public ICurrentTHolder<IIdentity> IdentityHolder => _unitOfWorkImplementation.IdentityHolder;
    }
}