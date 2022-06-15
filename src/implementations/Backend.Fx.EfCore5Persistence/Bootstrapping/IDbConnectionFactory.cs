using System.Data;

namespace Backend.Fx.EfCore5Persistence.Bootstrapping
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
}