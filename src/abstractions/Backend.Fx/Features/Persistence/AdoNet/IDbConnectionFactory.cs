using System.Data;

namespace Backend.Fx.Features.Persistence.AdoNet
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
}