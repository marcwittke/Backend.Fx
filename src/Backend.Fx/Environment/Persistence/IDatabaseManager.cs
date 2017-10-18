namespace Backend.Fx.Environment.Persistence
{
    /// <summary>
    /// Encapsulates database management. This interface hides the implementation details for creating, migrating and deleting the database
    /// </summary>
    public interface IDatabaseManager
    {
        void EnsureDatabaseExistence();
        void DeleteDatabase();
    }
}
