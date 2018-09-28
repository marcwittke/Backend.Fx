namespace Backend.Fx.Tests.Patterns.Jobs
{
    using System.Threading.Tasks;
    using Fx.Patterns.Jobs;
    using JetBrains.Annotations;
    using Xunit;

    public class TheJobExecutor
    {
        [UsedImplicitly]
        public class MyJob : IJob
        {
            public int ExecutionCount { get; set; }
            public void Run()
            {
                ExecutionCount++;
            }
        }

        private readonly MyJob _myJob = new MyJob();
        private readonly JobExecutor<MyJob> _sut;

        public TheJobExecutor()
        {
            _sut = new JobExecutor<MyJob>(_myJob);
        }

        [Fact]
        public void RunsTheJob()
        {
            _sut.ExecuteJob();
            Assert.Equal(1, _myJob.ExecutionCount);
        }

        [Fact]
        public async Task RunsTheJobAsynchronously()
        {
            await _sut.ExecuteJobAsync();
            Assert.Equal(1, _myJob.ExecutionCount);
        }
    }
}
