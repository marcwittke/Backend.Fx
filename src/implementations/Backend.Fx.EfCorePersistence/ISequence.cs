using System.Data;

namespace Backend.Fx.EfCorePersistence
{
    public interface ISequence
    {
        void EnsureSequence(IDbConnection dbConnection);
        int GetNextValue(IDbConnection dbConnection);
        int Increment { get; }
    }
}
