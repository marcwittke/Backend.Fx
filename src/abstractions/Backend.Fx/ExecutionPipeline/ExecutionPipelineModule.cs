using System.Security.Principal;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Extensions.MessageBus;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Backend.Fx.ExecutionPipeline
{
    internal class ExecutionPipelineModule : IModule
    {
        private readonly bool _withFrozenClockDuringExecution;

        public ExecutionPipelineModule(bool withFrozenClockDuringExecution = true)
        {
            _withFrozenClockDuringExecution = withFrozenClockDuringExecution;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            compositionRoot.Register(
                ServiceDescriptor.Singleton<IClock>(_ => SystemClock.Instance));

            if (_withFrozenClockDuringExecution)
            {
                compositionRoot.RegisterDecorator(
                    ServiceDescriptor.Scoped<IClock, FrozenClock>());
            }

            compositionRoot.Register(
                ServiceDescriptor.Scoped<IOperation, Operation>());

            compositionRoot.Register(
                ServiceDescriptor.Scoped<ICurrentTHolder<IIdentity>, CurrentIdentityHolder>());
            
            compositionRoot.Register(
                ServiceDescriptor.Scoped<ICurrentTHolder<Correlation>, CurrentCorrelationHolder>());
        }
    }
}