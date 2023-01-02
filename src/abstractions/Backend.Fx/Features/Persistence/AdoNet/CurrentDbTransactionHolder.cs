using System.Data;
using Backend.Fx.Util;
using JetBrains.Annotations;

namespace Backend.Fx.Features.Persistence.AdoNet
{
    [UsedImplicitly]
    public class CurrentDbTransactionHolder : CurrentTHolder<IDbTransaction>, ICurrentTHolder<IDbTransaction>
    {
        public override IDbTransaction ProvideInstance()
        {
            return null;
        }

        protected override string Describe(IDbTransaction instance)
        {
            return instance == null ? "<NULL>" : "DbTransaction";
        }
    }
}