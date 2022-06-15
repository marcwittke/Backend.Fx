using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.Jobs;
using Backend.Fx.Tests.Patterns.DependencyInjection;
using Backend.Fx.Tests.Patterns.Jobs;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.Authorization
{
    public class TheAuthorizingApplication : TestWithLogging
    {
        public TheAuthorizingApplication(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveAuthorizationTypes(CompositionRootType compositionRootType)
        {
            var sut = new AuthorizingApplication(
                new BackendFxApplication(
                    compositionRootType.Create(),
                    new ExceptionLoggers(),
                    typeof(TheAuthorizingApplication).Assembly));

            await sut.BootAsync();

            sut.Invoker.Invoke(sp =>
                {
                    var auth = sp.GetRequiredService<IAggregateAuthorization<BackendFxAggregate>>();
                    Assert.IsType<Auth>(auth);
                },
                new SystemIdentity(),
                new TenantId(345));
        }
    }

    public class Auth : DenyAll<BackendFxAggregate>
    {
        public override Expression<Func<BackendFxAggregate, bool>> HasAccessExpression => bfa => true;
    }
}