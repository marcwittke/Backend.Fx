namespace Backend.Fx.Patterns.IdGeneration
{
    public interface ISequence
    {
        void EnsureSequence();
        int GetNextValue();
        int Increment { get; }
    }
}