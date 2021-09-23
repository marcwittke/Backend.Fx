using System.Linq;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Tests.BuildingBlocks;
using Xunit;

namespace Backend.Fx.Tests.Patterns.Authorization
{
    public class TheDenyAllImplementation
    {
        private readonly IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot> _sut
            = new DenyAll<TheAggregateRoot.TestAggregateRoot>();

        private readonly TheAggregateRoot.TestAggregateRoot _testAggregateRoot = new(1, "e");

        [Fact]
        public void DeniesAccess()
        {
            Assert.False(_sut.HasAccessExpression.Compile().Invoke(_testAggregateRoot));
            Assert.Empty(_sut.Filter(new[] { _testAggregateRoot }.AsQueryable()));
        }

        [Fact]
        public void DeniesCreation()
        {
            Assert.False(_sut.CanCreate(_testAggregateRoot));
        }

        [Fact]
        public void DeniesModification()
        {
            Assert.False(_sut.CanModify(_testAggregateRoot));
        }

        [Fact]
        public void DeniesDeletion()
        {
            Assert.False(_sut.CanDelete(_testAggregateRoot));
        }
    }
}
