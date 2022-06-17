using JetBrains.Annotations;

namespace Backend.Fx.Patterns.IdGeneration
{
    [PublicAPI]
    public interface ISequence
    {
        void EnsureSequence();
        int GetNextValue();
        int Increment { get; }
    }
}