using System.Data;

namespace Backend.Fx.EfCorePersistence.Bootstrapping
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
}