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

        private readonly MyJob myJob = new MyJob();
        private readonly JobExecutor<MyJob> sut;

        public TheJobExecutor()
        {
            sut = new JobExecutor<MyJob>(myJob);
        }

        [Fact]
        public void RunsTheJob()
        {
            sut.ExecuteJob();
            Assert.Equal(1, myJob.ExecutionCount);
        }

        [Fact]
        public async Task RunsTheJobAsynchronously()
        {
            await sut.ExecuteJobAsync();
            Assert.Equal(1, myJob.ExecutionCount);
        }
    }
}
