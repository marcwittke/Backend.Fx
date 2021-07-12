namespace Backend.Fx.Patterns.DependencyInjection
{
    public class CurrentCorrelationHolder : CurrentTHolder<Correlation>
    {
        public CurrentCorrelationHolder()
        {
        }

        public CurrentCorrelationHolder(Correlation initial) : base(initial)
        {
        }

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