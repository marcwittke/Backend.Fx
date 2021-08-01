using System.Data;

namespace Backend.Fx.Environment.Persistence
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
}