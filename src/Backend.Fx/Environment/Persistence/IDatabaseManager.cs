namespace Backend.Fx.Environment.Persistence
{
    public interface IDatabaseManager
    {
        bool DatabaseExists { get; }
        void EnsureDatabaseExistence();
        void DeleteDatabase();
    }
}
