using System.Threading;

namespace Backend.Fx.ExecutionPipeline
{
    public class OperationCounter
    {
        private int _count;
        
        public int Count()
        {
            return Interlocked.Increment(ref _count);
        }
    }
}