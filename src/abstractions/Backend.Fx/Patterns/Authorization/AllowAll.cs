using System;
using System.Linq.Expressions;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Logging;

namespace Backend.Fx.Patterns.Authorization
{
    public sealed class AllowAll<TAggregateRoot> : AggregateAuthorization<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private static readonly ILogger Logger = LogManager.Create<AggregateAuthorization<TAggregateRoot>>();
        
        public AllowAll()
        {
            Logger.Info($"Authorization for {typeof(TAggregateRoot).Name} is set to 'allow all'");
        }
        
        public override Expression<Func<TAggregateRoot, bool>> HasAccessExpression
        {
            get { return agg => true; }
        }

        public override bool CanCreate(TAggregateRoot t)
        {
            return true;
        }
    }
}