using JetBrains.Annotations;

namespace Backend.Fx.Features.Persistence
{
    [PublicAPI]
    public interface ISequence
    {
        void EnsureSequence();
        int GetNextValue();
        int Increment { get; }
    }
}