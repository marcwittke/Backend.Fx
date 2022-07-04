using System.Linq;
using Backend.Fx.Features.Authorization;
using Backend.Fx.Tests.BuildingBlocks;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.Authorization
{
    public class TheAllowAllImplementation : TestWithLogging
    {
        private readonly IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot> _sut = new AllowAll<TheAggregateRoot.TestAggregateRoot>();
        private readonly TheAggregateRoot.TestAggregateRoot _testAggregateRoot = new TheAggregateRoot.TestAggregateRoot(1,"e");

        [Fact]
        public void AllowsAccess()
        {
            Assert.True(_sut.HasAccessExpression.Compile().Invoke(_testAggregateRoot));
            Assert.Contains(_testAggregateRoot, _sut.Filter(new[] {_testAggregateRoot}.AsQueryable()));
        }

        [Fact]
        public void AllowsCreation()
        {
            Assert.True(_sut.CanCreate(_testAggregateRoot));
        }
        
        [Fact]
        public void AllowsModification()
        {
            Assert.True(_sut.CanModify(_testAggregateRoot));
        }
        
        [Fact]
        public void AllowsDeletion()
        {
            Assert.True(_sut.CanDelete(_testAggregateRoot));
        }

        public TheAllowAllImplementation(ITestOutputHelper output) : base(output)
        {
        }
    }
}