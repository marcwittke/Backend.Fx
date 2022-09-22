using Backend.Fx.Extensions.MessageBus;
using Backend.Fx.Util;
using JetBrains.Annotations;

namespace Backend.Fx.ExecutionPipeline
{
    [PublicAPI]
    public sealed class CurrentCorrelationHolder : CurrentTHolder<Correlation>
    {
        public override Correlation ProvideInstance()
        {
            return new Correlation();
        }

        protected override string Describe(Correlation instance)
        {
            return $"Correlation: {instance?.Id.ToString() ?? "NULL"}";
        }
    }
}