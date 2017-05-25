namespace Backend.Fx.Tests.BuildingBlocks
{
    using System;
    using System.Linq;
    using FakeItEasy;
    using Fx.Environment.MultiTenancy;
    using Fx.Patterns.Authorization;
    using Fx.Patterns.DependencyInjection;
    using Testing;
    using Xunit;

    public class TestAggregateAuthorization : AllowAll<TheAggregateRoot.TestAggregateRoot> { }

    public class TheRepository
    {
        [Fact]
        public void ThrowsOnAddWhenTenantIdHolderIsEmpty()
        {
            var tenantHolder = A.Fake<ICurrentTHolder<TenantId>>();
            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(null));

            var authorization = A.Fake<IAggregateRootAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(tenantHolder, authorization);
            
            A.CallTo(() => authorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => authorization.CanCreate()).Returns(true);
            Assert.Throws<InvalidOperationException>(() => sut.Add(new TheAggregateRoot.TestAggregateRoot("whatever")));

            // even when I don't have permissions
            A.CallTo(() => authorization.HasAccessExpression).Returns(setting => false);
            A.CallTo(() => authorization.CanCreate()).Returns(false);
            Assert.Throws<InvalidOperationException>(() => sut.Add(new TheAggregateRoot.TestAggregateRoot("whatever")));
        }

        [Fact]
        public void ThrowsOnDeleteWhenTenantIdHolderIsEmpty()
        {
            var tenantHolder = A.Fake<ICurrentTHolder<TenantId>>();
            
            var authorization = A.Fake<IAggregateRootAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => authorization.CanCreate()).Returns(true);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(tenantHolder, authorization);

            var agg1 = new TheAggregateRoot.TestAggregateRoot("whatever") { Id = 12123123, TenantId = 234 };
            sut.Store.Add(agg1.Id, agg1);

            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(null));
            Assert.Throws<InvalidOperationException>(() => sut.Delete(agg1));
        }

        [Fact]
        public void ThrowsOnGetWhenTenantIdHolderIsEmpty()
        {
            var tenantHolder = A.Fake<ICurrentTHolder<TenantId>>();
            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(null));

            var authorization = A.Fake<IAggregateRootAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(tenantHolder, authorization);

            A.CallTo(() => authorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => authorization.CanCreate()).Returns(true);
            Assert.Throws<InvalidOperationException>(() => sut.AggregateQueryable.ToArray());

            // even when I don't have permissions
            A.CallTo(() => authorization.HasAccessExpression).Returns(setting => false);
            A.CallTo(() => authorization.CanCreate()).Returns(false);
            Assert.Throws<InvalidOperationException>(() => sut.AggregateQueryable.ToArray());
        }

        [Fact]
        public void MaintainsTenantIdOnAdd()
        {
            var authorization = A.Fake<IAggregateRootAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => authorization.CanCreate()).Returns(true);

            var tenantHolder = A.Fake<ICurrentTHolder<TenantId>>();
            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(tenantHolder, authorization);

            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(234));
            var agg1 = new TheAggregateRoot.TestAggregateRoot("1");
            sut.Add(agg1);
            Assert.Equal(234, agg1.TenantId);

            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(235));
            var agg2 = new TheAggregateRoot.TestAggregateRoot("2");
            sut.Add(agg2);
            Assert.Equal(235, agg2.TenantId);
        }

        [Fact]
        public void DoesNotReturnItemsFromOtherTenants()
        {
            var authorization = A.Fake<IAggregateRootAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => authorization.CanCreate()).Returns(true);

            var tenantHolder = A.Fake<ICurrentTHolder<TenantId>>();
            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(tenantHolder, authorization);

            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(234));
            sut.Add(new TheAggregateRoot.TestAggregateRoot("1"));
            sut.Add(new TheAggregateRoot.TestAggregateRoot("2"));
            sut.Add(new TheAggregateRoot.TestAggregateRoot("3"));

            // now I am in another tenant
            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(233));
            Assert.Empty(sut.AggregateQueryable);
        }

        [Fact]
        public void ReturnsOnlyItemsFromMyTenant()
        {
            var tenantHolder = A.Fake<ICurrentTHolder<TenantId>>();
            var authorization = A.Fake<IAggregateRootAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => authorization.CanCreate()).Returns(true);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(tenantHolder, authorization);
            var agg1 = new TheAggregateRoot.TestAggregateRoot("1");
            var agg2 = new TheAggregateRoot.TestAggregateRoot("2");
            var agg3 = new TheAggregateRoot.TestAggregateRoot("3");
            var agg4 = new TheAggregateRoot.TestAggregateRoot("4");
            var agg5 = new TheAggregateRoot.TestAggregateRoot("5");

            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(234));
            sut.Add(agg1);
            sut.Add(agg2);
            sut.Add(agg3);

            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(567));
            sut.Add(agg4);
            sut.Add(agg5);

            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(234));
            Assert.Equal(3, sut.AggregateQueryable.Count());
            Assert.Contains(agg1, sut.AggregateQueryable);
            Assert.Contains(agg2, sut.AggregateQueryable);
            Assert.Contains(agg3, sut.AggregateQueryable);

            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(567));
            Assert.Equal(2, sut.AggregateQueryable.Count());
            Assert.Contains(agg4, sut.AggregateQueryable);
            Assert.Contains(agg5, sut.AggregateQueryable);
        }

        [Fact]
        public void DeletesItemFromMyTenant()
        {
            var tenantHolder = A.Fake<ICurrentTHolder<TenantId>>();

            var authorization = A.Fake<IAggregateRootAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => authorization.CanCreate()).Returns(true);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(tenantHolder, authorization);

            var agg1 = new TheAggregateRoot.TestAggregateRoot("whatever") { Id = 12123123, TenantId = 234 };
            sut.Store.Add(agg1.Id, agg1);

            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(234));
            sut.Delete(agg1);

            Assert.Empty(sut.Store);
        }

        [Fact]
        public void ReturnsAll()
        {
            var tenantHolder = A.Fake<ICurrentTHolder<TenantId>>();
            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(234));

            var authorization = A.Fake<IAggregateRootAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => authorization.CanCreate()).Returns(true);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(tenantHolder, authorization);
            var agg1 = new TheAggregateRoot.TestAggregateRoot("whatever") { Id = 12123123, TenantId = 234 };
            var agg2 = new TheAggregateRoot.TestAggregateRoot("whatever") { Id = 12123124, TenantId = 234 };
            var agg3 = new TheAggregateRoot.TestAggregateRoot("whatever") { Id = 12123125, TenantId = 234 };
            var agg4 = new TheAggregateRoot.TestAggregateRoot("whatever") { Id = 12123126, TenantId = 234 };

            sut.Add(agg1);
            sut.Add(agg2);
            sut.Add(agg3);
            sut.Add(agg4);

            Assert.Equal(4, sut.GetAll().Length);
            Assert.Contains(agg1, sut.GetAll());
            Assert.Contains(agg2, sut.GetAll());
            Assert.Contains(agg3, sut.GetAll());
            Assert.Contains(agg4, sut.GetAll());
        }

        [Fact]
        public void CanResolveListOfIds()
        {
            var tenantHolder = A.Fake<ICurrentTHolder<TenantId>>();
            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(234));

            var authorization = A.Fake<IAggregateRootAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => authorization.CanCreate()).Returns(true);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(tenantHolder, authorization);
            var agg1 = new TheAggregateRoot.TestAggregateRoot("whatever") { Id = 23, TenantId = 234 };
            var agg2 = new TheAggregateRoot.TestAggregateRoot("whatever") { Id = 24, TenantId = 234 };
            var agg3 = new TheAggregateRoot.TestAggregateRoot("whatever") { Id = 25, TenantId = 234 };
            var agg4 = new TheAggregateRoot.TestAggregateRoot("whatever") { Id = 26, TenantId = 234 };

            sut.Store.Add(agg1.Id, agg1);
            sut.Store.Add(agg2.Id, agg2);
            sut.Store.Add(agg3.Id, agg3);
            sut.Store.Add(agg4.Id, agg4);
            
            var resolved = sut.Resolve(new[] { 23, 24, 25, 26 });
            Assert.Equal(4, resolved.Length);
            Assert.Contains(agg1, resolved);
            Assert.Contains(agg2, resolved);
            Assert.Contains(agg3, resolved);
            Assert.Contains(agg4, resolved);
        }

        [Fact]
        public void ThrowsOnResolveWhenTenantDoesNotMatch()
        {
            var tenantHolder = A.Fake<ICurrentTHolder<TenantId>>();
            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(234));

            var authorization = A.Fake<IAggregateRootAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => authorization.CanCreate()).Returns(true);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(tenantHolder, authorization);
            var agg1 = new TheAggregateRoot.TestAggregateRoot("whatever") { Id = 23, TenantId = 234 };
            var agg2 = new TheAggregateRoot.TestAggregateRoot("whatever") { Id = 24, TenantId = 234 };
            var agg3 = new TheAggregateRoot.TestAggregateRoot("whatever") { Id = 25, TenantId = 234 };
            var agg4 = new TheAggregateRoot.TestAggregateRoot("whatever") { Id = 26, TenantId = 999 };

            sut.Store.Add(agg1.Id, agg1);
            sut.Store.Add(agg2.Id, agg2);
            sut.Store.Add(agg3.Id, agg3);
            sut.Store.Add(agg4.Id, agg4);

            Assert.Throws<ArgumentException>(()=>sut.Resolve(new[] { 23, 24, 25, 26 }));
        }
    }
}