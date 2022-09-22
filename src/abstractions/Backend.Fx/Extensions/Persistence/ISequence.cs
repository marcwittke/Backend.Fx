using JetBrains.Annotations;

namespace Backend.Fx.Extensions.Persistence
{
    [PublicAPI]
    public interface ISequence
    {
        void EnsureSequence();
        int GetNextValue();
        int Increment { get; }
    }
}