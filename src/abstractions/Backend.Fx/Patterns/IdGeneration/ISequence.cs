namespace Backend.Fx.Patterns.IdGeneration
{
    public interface ISequence
    {
        int Increment { get; }

        void EnsureSequence();
        int GetNextValue();
    }
}
