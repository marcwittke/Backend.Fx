using System.Data;

namespace Backend.Fx.EfCore6Persistence.Bootstrapping
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
}