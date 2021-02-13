namespace Backend.Fx.Patterns.DataGeneration
{
    /// <summary>
    /// Marks an <see cref="DataGenerator"/> as active in development environments only
    /// </summary>
    public interface IDemoDataGenerator : IDataGenerator
    {
    }
}