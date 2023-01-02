using JetBrains.Annotations;

namespace Backend.Fx.Features.Persistence
{
    public interface ICanFlush
    {
        void Flush();
    }

    [UsedImplicitly]
    internal class DefaultFlush : ICanFlush
    {
        public void Flush()
        { }
    }
}