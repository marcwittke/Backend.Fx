namespace Backend.Fx.SqlServer
{
    using System.Data;
    using System.Data.SqlClient;
    using Backend.Fx.EfCorePersistence.Bootstrapping;

    public class MsSqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public MsSqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection Create()
        {
            var sqlConnection = new SqlConnection(_connectionString);
            return sqlConnection;
        }
    }
}
