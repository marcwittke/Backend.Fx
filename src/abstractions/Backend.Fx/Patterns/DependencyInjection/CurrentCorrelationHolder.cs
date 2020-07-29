namespace Backend.Fx.Patterns.DependencyInjection
{
    public class CurrentCorrelationHolder : CurrentTHolder<Correlation>
    {
        public override Correlation ProvideInstance()
        {
            return new Correlation();
        }

        protected override string Describe(Correlation instance)
        {
            if (instance == null)
            {
                return "<NULL>";
            }

            return $"Correlation: {instance.Id}";
        }
    }
}