﻿using System;
using System.Linq.Expressions;
using Backend.Fx.Domain;

namespace Backend.Fx.Features.Authorization
{
    public class DenyAll<TAggregateRoot> : AuthorizationPolicy<TAggregateRoot> 
        where TAggregateRoot : IAggregateRoot
    {
        public override Expression<Func<TAggregateRoot, bool>> HasAccessExpression
        {
            get { return agg => false; }
        }

        public override bool CanCreate(TAggregateRoot t)
        {
            return false;
        }
    }
}