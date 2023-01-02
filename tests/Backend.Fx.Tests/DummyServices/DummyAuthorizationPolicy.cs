using System;
using System.Linq.Expressions;
using Backend.Fx.Features.Authorization;
using JetBrains.Annotations;

namespace Backend.Fx.Tests.DummyServices;

[UsedImplicitly]
public class DummyAuthorizationPolicy : IAuthorizationPolicy<DummyAggregate>
{
    private readonly IDummyAuthorizationPolicySpy _spy;

    public DummyAuthorizationPolicy(IDummyAuthorizationPolicySpy spy)
    {
        _spy = spy;
    }


    public Expression<Func<DummyAggregate, bool>> HasAccessExpression => _spy.HasAccessExpression;

    public bool CanCreate(DummyAggregate t)
    {
        return _spy.CanCreate(t);
    }

    public bool CanModify(DummyAggregate t)
    {
        return _spy.CanModify(t);
    }

    public bool CanDelete(DummyAggregate t)
    {
        return _spy.CanDelete(t);
    }
}

public interface IDummyAuthorizationPolicySpy : IAuthorizationPolicy<DummyAggregate>
{ }