using JetBrains.Annotations;

namespace Backend.Fx.Features.IdGeneration
{
    [PublicAPI]
    public interface ISequence
    {
        void EnsureSequence();
        int GetNextValue();
        int Increment { get; }
    }
}