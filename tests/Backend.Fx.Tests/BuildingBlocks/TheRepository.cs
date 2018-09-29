using Backend.Fx.InMemoryPersistence;
using Xunit;

namespace Backend.Fx.Tests.BuildingBlocks
{
    using System;
    using System.Linq;
    using System.Security;
    using FakeItEasy;
    using Fx.Environment.MultiTenancy;
    using Fx.Exceptions;
    using Fx.Patterns.Authorization;

    public class TheRepository
    {
        [Fact]
        public void ReturnsByIdOnSingle()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(false);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(234), authorization);
            var agg1 = new TheAggregateRoot.TestAggregateRoot(23, "whatever") { TenantId = 234 };
            var agg2 = new TheAggregateRoot.TestAggregateRoot(24, "whatever") { TenantId = 234 };
            var agg3 = new TheAggregateRoot.TestAggregateRoot(25, "whatever") { TenantId = 234 };
            var agg4 = new TheAggregateRoot.TestAggregateRoot(26, "whatever") { TenantId = 234 };

            sut.Store.Add(agg1.Id, agg1);
            sut.Store.Add(agg2.Id, agg2);
            sut.Store.Add(agg3.Id, agg3);
            sut.Store.Add(agg4.Id, agg4);

            Assert.Equal(agg1, sut.Single(agg1.Id));
            Assert.Equal(agg2, sut.Single(agg2.Id));
            Assert.Equal(agg3, sut.Single(agg3.Id));
            Assert.Equal(agg4, sut.Single(agg4.Id));
            Assert.Throws<NotFoundException<TheAggregateRoot.TestAggregateRoot>>(() => sut.Single(235421354));
        }

        [Fact]
        public void ReturnsByIdOnSingleOrDefault()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(false);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(234), authorization);
            var agg1 = new TheAggregateRoot.TestAggregateRoot(23, "whatever") { TenantId = 234 };
            var agg2 = new TheAggregateRoot.TestAggregateRoot(24, "whatever") { TenantId = 234 };
            var agg3 = new TheAggregateRoot.TestAggregateRoot(25, "whatever") { TenantId = 234 };
            var agg4 = new TheAggregateRoot.TestAggregateRoot(26, "whatever") { TenantId = 234 };

            sut.Store.Add(agg1.Id, agg1);
            sut.Store.Add(agg2.Id, agg2);
            sut.Store.Add(agg3.Id, agg3);
            sut.Store.Add(agg4.Id, agg4);

            Assert.Equal(agg1, sut.SingleOrDefault(agg1.Id));
            Assert.Equal(agg2, sut.SingleOrDefault(agg2.Id));
            Assert.Equal(agg3, sut.SingleOrDefault(agg3.Id));
            Assert.Equal(agg4, sut.SingleOrDefault(agg4.Id));
            Assert.Null(sut.SingleOrDefault(235421354));
        }

        [Fact]
        public void ProvidesCorrectAny()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(false);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(234), authorization);
            Assert.False(sut.Any());

            var agg1 = new TheAggregateRoot.TestAggregateRoot(23, "whatever") { TenantId = 234 };
            var agg2 = new TheAggregateRoot.TestAggregateRoot(24, "whatever") { TenantId = 234 };
            var agg3 = new TheAggregateRoot.TestAggregateRoot(25, "whatever") { TenantId = 234 };
            var agg4 = new TheAggregateRoot.TestAggregateRoot(26, "whatever") { TenantId = 234 };

            sut.Store.Add(agg1.Id, agg1);
            sut.Store.Add(agg2.Id, agg2);
            sut.Store.Add(agg3.Id, agg3);
            sut.Store.Add(agg4.Id, agg4);

            Assert.True(sut.Any());
        }

        [Fact]
        public void ThrowsOnAddWhenTenantIdIsEmpty()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(null), authorization);

            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            Assert.Throws<InvalidOperationException>(() => sut.Add(new TheAggregateRoot.TestAggregateRoot(77, "whatever")));

            // even when I don't have permissions
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => false);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(false);
            Assert.Throws<SecurityException>(() => sut.Add(new TheAggregateRoot.TestAggregateRoot(78, "whatever")));
        }

        [Fact]
        public void ThrowsOnDeleteWhenTenantIdHolderIsEmpty()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(true);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(null), authorization);

            var agg1 = new TheAggregateRoot.TestAggregateRoot(12123123, "whatever") { TenantId = 234 };
            sut.Store.Add(agg1.Id, agg1);

            Assert.Throws<InvalidOperationException>(() => sut.Delete(agg1));
        }

        [Fact]
        public void ThrowsOnDeleteWhenUnauthorized()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(true);
            A.CallTo(() => authorization.CanDelete(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(false);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(234), authorization);

            var agg1 = new TheAggregateRoot.TestAggregateRoot(12123123, "whatever") { TenantId = 234 };
            sut.Store.Add(agg1.Id, agg1);

            Assert.Throws<SecurityException>(() => sut.Delete(agg1));
        }

        [Fact]
        public void ReturnsEmptyWhenTenantIdHolderIsEmpty()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(null), authorization);

            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(true);
            Assert.Empty(sut.AggregateQueryable);
        }

        [Fact]
        public void MaintainsTenantIdOnAdd()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(true);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(234), authorization);

            var agg1 = new TheAggregateRoot.TestAggregateRoot(22,"1");
            sut.Add(agg1);
            Assert.Equal(234, agg1.TenantId);
        }

        [Fact]
        public void DoesNotReturnItemsFromOtherTenants()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(true);

            var store = new InMemoryStore<TheAggregateRoot.TestAggregateRoot>();
            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(store, CurrentTenantIdHolder.Create(234), authorization);

            sut.Add(new TheAggregateRoot.TestAggregateRoot(22, "1"));
            sut.Add(new TheAggregateRoot.TestAggregateRoot(23, "2"));
            sut.Add(new TheAggregateRoot.TestAggregateRoot(24, "3"));

            // now I am in another tenant
            sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(store, CurrentTenantIdHolder.Create(233), authorization);
            Assert.Empty(sut.AggregateQueryable);
        }

        [Fact]
        public void ReturnsOnlyItemsFromMyTenant()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(true);

            var agg1 = new TheAggregateRoot.TestAggregateRoot(11, "1");
            var agg2 = new TheAggregateRoot.TestAggregateRoot(12, "2");
            var agg3 = new TheAggregateRoot.TestAggregateRoot(13, "3");
            var agg4 = new TheAggregateRoot.TestAggregateRoot(14, "4");
            var agg5 = new TheAggregateRoot.TestAggregateRoot(15, "5");

            var store = new InMemoryStore<TheAggregateRoot.TestAggregateRoot>();
            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(store, CurrentTenantIdHolder.Create(234), authorization);
            sut.Add(agg1);
            sut.Add(agg2);
            sut.Add(agg3);

            sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(store, CurrentTenantIdHolder.Create(567), authorization);
            sut.Add(agg4);
            sut.Add(agg5);

            sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(store, CurrentTenantIdHolder.Create(234), authorization);
            Assert.Equal(3, sut.AggregateQueryable.Count());
            Assert.Contains(agg1, sut.AggregateQueryable);
            Assert.Contains(agg2, sut.AggregateQueryable);
            Assert.Contains(agg3, sut.AggregateQueryable);

            sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(store, CurrentTenantIdHolder.Create(567), authorization);
            Assert.Equal(2, sut.AggregateQueryable.Count());
            Assert.Contains(agg4, sut.AggregateQueryable);
            Assert.Contains(agg5, sut.AggregateQueryable);
        }

        [Fact]
        public void DeletesItemFromMyTenant()
        {

            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(true);
            A.CallTo(() => authorization.CanDelete(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(true);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(234), authorization);

            var agg1 = new TheAggregateRoot.TestAggregateRoot(12123123, "whatever") { TenantId = 234 };
            sut.Store.Add(agg1.Id, agg1);

            sut.Delete(agg1);

            Assert.Empty(sut.Store);
        }

        [Fact]
        public void ReturnsAll()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(true);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(234), authorization);
            var agg1 = new TheAggregateRoot.TestAggregateRoot(12123123, "whatever") { TenantId = 234 };
            var agg2 = new TheAggregateRoot.TestAggregateRoot(12123124, "whatever") { TenantId = 234 };
            var agg3 = new TheAggregateRoot.TestAggregateRoot(12123125, "whatever") { TenantId = 234 };
            var agg4 = new TheAggregateRoot.TestAggregateRoot(12123126, "whatever") { TenantId = 234 };

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
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(true);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(234), authorization);
            var agg1 = new TheAggregateRoot.TestAggregateRoot(23, "whatever") { TenantId = 234 };
            var agg2 = new TheAggregateRoot.TestAggregateRoot(24, "whatever") { TenantId = 234 };
            var agg3 = new TheAggregateRoot.TestAggregateRoot(25, "whatever") { TenantId = 234 };
            var agg4 = new TheAggregateRoot.TestAggregateRoot(26, "whatever") { TenantId = 234 };

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
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(true);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(234), authorization);
            var agg1 = new TheAggregateRoot.TestAggregateRoot(23, "whatever") { TenantId = 234 };
            var agg2 = new TheAggregateRoot.TestAggregateRoot(24, "whatever") { TenantId = 234 };
            var agg3 = new TheAggregateRoot.TestAggregateRoot(25, "whatever") { TenantId = 234 };
            var agg4 = new TheAggregateRoot.TestAggregateRoot(26, "whatever") { TenantId = 999 };

            sut.Store.Add(agg1.Id, agg1);
            sut.Store.Add(agg2.Id, agg2);
            sut.Store.Add(agg3.Id, agg3);
            sut.Store.Add(agg4.Id, agg4);

            Assert.Throws<ArgumentException>(() => sut.Resolve(new[] { 23, 24, 25, 26 }));
        }

        [Fact]
        public void ThrowsOnDeleteWhenTenantDoesNotMatch()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(true);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(234), authorization);
            var agg1 = new TheAggregateRoot.TestAggregateRoot(23, "whatever") { TenantId = 234 };
            var agg2 = new TheAggregateRoot.TestAggregateRoot(24, "whatever") { TenantId = 234 };
            var agg3 = new TheAggregateRoot.TestAggregateRoot(25, "whatever") { TenantId = 234 };
            var agg4 = new TheAggregateRoot.TestAggregateRoot(26, "whatever") { TenantId = 999 };

            sut.Store.Add(agg1.Id, agg1);
            sut.Store.Add(agg2.Id, agg2);
            sut.Store.Add(agg3.Id, agg3);
            sut.Store.Add(agg4.Id, agg4);

            Assert.Throws<SecurityException>(() => sut.Delete(agg4));
        }

        [Fact]
        public void ThrowsOnAddWhenUnauthorized()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(234), authorization);

            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(false);
            Assert.Throws<SecurityException>(() => sut.Add(new TheAggregateRoot.TestAggregateRoot(44, "whatever")));
        }

        [Fact]
        public void ReturnsOnlyAuthorizedRecords()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q.Where(agg => agg.Id == 25 || agg.Id == 26));
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(false);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(234), authorization);
            var agg1 = new TheAggregateRoot.TestAggregateRoot(23, "whatever") { TenantId = 234 };
            var agg2 = new TheAggregateRoot.TestAggregateRoot(24, "whatever") { TenantId = 234 };
            var agg3 = new TheAggregateRoot.TestAggregateRoot(25, "whatever") { TenantId = 234 };
            var agg4 = new TheAggregateRoot.TestAggregateRoot(26, "whatever") { TenantId = 234 };

            sut.Store.Add(agg1.Id, agg1);
            sut.Store.Add(agg2.Id, agg2);
            sut.Store.Add(agg3.Id, agg3);
            sut.Store.Add(agg4.Id, agg4);

            var all = sut.GetAll();
            Assert.Equal(2, all.Length);
            Assert.DoesNotContain(agg1, all);
            Assert.DoesNotContain(agg2, all);
            Assert.Contains(agg3, all);
            Assert.Contains(agg4, all);
        }

        [Fact]
        public void AcceptsNullArrayToResolve()
        {
            var authorization = A.Fake<IAggregateAuthorization<TheAggregateRoot.TestAggregateRoot>>();
            A.CallTo(() => authorization.HasAccessExpression).Returns(agg => true);
            A.CallTo(() => authorization.Filter(A<IQueryable<TheAggregateRoot.TestAggregateRoot>>._)).ReturnsLazily((IQueryable<TheAggregateRoot.TestAggregateRoot> q) => q);
            A.CallTo(() => authorization.CanCreate(A<TheAggregateRoot.TestAggregateRoot>._)).Returns(true);

            var sut = new InMemoryRepository<TheAggregateRoot.TestAggregateRoot>(new InMemoryStore<TheAggregateRoot.TestAggregateRoot>(), CurrentTenantIdHolder.Create(234), authorization);
            Assert.Empty(sut.Resolve(null));
        }
    }
}