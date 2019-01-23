using System.Collections.Generic;

namespace Backend.Fx.EfCorePersistence.Bootstrapping
{
    public interface IDatabaseBootstrapperInstanceProvider
    {
        IEnumerable<IFullTextSearchIndex> GetAllSearchIndizes();
        IEnumerable<ISequence> GetAllSequences();
    }
}