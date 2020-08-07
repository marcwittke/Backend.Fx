using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Bootstrapping
{
    public class AnApplication : BackendFxApplication
    {
        private readonly ITenantRepository _tenantRepository;
        private static readonly ILogger Logger = LogManager.Create<AnApplication>();

        public AnApplication(ICompositionRoot compositionRoot)
            : this(compositionRoot, new InMemoryTenantRepository())
        {
        }

        public AnApplication(ICompositionRoot compositionRoot, ITenantRepository tenantRepository)
            : base(compositionRoot,
                   new InMemoryMessageBus(new BackendFxApplicationInvoker(compositionRoot, A.Fake<IExceptionLogger>())))
        {
            _tenantRepository = tenantRepository;
        }

        public void EnsureProdTenant()
        {
            var prodTenantId = TenantIdService.GetActiveProductionTenantIds().FirstOrDefault();
            if (prodTenantId == null)
            {
                ManualResetEventSlim prodTenantActivated = new ManualResetEventSlim(false);
                var messageBus = CompositionRoot.GetInstance<IMessageBus>();
                messageBus.Subscribe(new DelegateIntegrationMessageHandler<TenantActivated>(ta => prodTenantActivated.Set()));
                ProdTenantId = new TenantService(messageBus, _tenantRepository).CreateProductionTenant("prod", "unit test created", "en-US");
                Assert.True(prodTenantActivated.Wait(Debugger.IsAttached ? int.MaxValue : 10000));
            }
            else
            {
                ProdTenantId = new TenantId(prodTenantId.Value);
            }
        }

        public void EnsureDemoTenant()
        {
            TenantId demoTenantId = TenantIdService.GetActiveDemonstrationTenantIds().FirstOrDefault();
            if (demoTenantId == null)
            {
                var demoTenantActivated = new ManualResetEventSlim(false);
                MessageBus.Subscribe(new DelegateIntegrationMessageHandler<TenantActivated>(ta => demoTenantActivated.Set()));
                DemoTenantId = new TenantService(MessageBus, _tenantRepository).CreateTenant("demo", "unit test created", "en-US");
                Assert.True(demoTenantActivated.Wait(Debugger.IsAttached ? int.MaxValue : 10000));
            }
            else
            {
                DemoTenantId = new TenantId(demoTenantId.Value);
            }
        }

        public TenantId ProdTenantId { get; private set; }

        public TenantId DemoTenantId { get; private set; }

        protected override Task OnBooted(CancellationToken cancellationToken)
        {
            var tenantService = new TenantService(MessageBus, _tenantRepository);
            this.RegisterSeedActionForNewlyCreatedTenants(tenantService);

            new DataGenerationContext(new TenantIdService(), CompositionRoot,)
            this.SeedDataForAllActiveTenants();
            return Task.CompletedTask;
        }
    }
}