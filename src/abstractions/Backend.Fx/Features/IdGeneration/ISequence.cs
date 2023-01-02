using JetBrains.Annotations;

namespace Backend.Fx.Features.IdGeneration
{
    [PublicAPI]
    public interface ISequence<out TId> 
    {
        void EnsureSequence();
        TId GetNextValue();
        TId Increment { get; }
    }
}