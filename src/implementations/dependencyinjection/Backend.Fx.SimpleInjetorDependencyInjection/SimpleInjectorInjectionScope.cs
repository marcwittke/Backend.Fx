using Backend.Fx.Patterns.DependencyInjection;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection
{
    public class SimpleInjectorInjectionScope : InjectionScope
    {
        private readonly Scope _scope;

        public SimpleInjectorInjectionScope(int sequenceNumber, Scope scope) : base(sequenceNumber)
        {
            _scope = scope;
            InstanceProvider = new SimpleInjectorInstanceProvider(_scope.Container);
        }

        public override IInstanceProvider InstanceProvider { get; }

        public override void Dispose()
        {
            _scope.Dispose();
        }
    }
}
