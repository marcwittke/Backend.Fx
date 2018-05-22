namespace Backend.Fx.Tests.Patterns.Jobs
{
    using System;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Fx.Patterns.Jobs;
    using Fx.Patterns.UnitOfWork;
    using JetBrains.Annotations;
    using Xunit;

    public class TheUnitOfWorkJobExecutor
    {
        [UsedImplicitly]
        public class MyJob : IJob
        {
            public void Run()
            { }
        }

        private readonly UnitOfWorkJobExecutor<MyJob> sut;
        private readonly IUnitOfWork unitOfWork;
        private readonly IJobExecutor<MyJob> jobExecutor;

        public TheUnitOfWorkJobExecutor()
        {
            unitOfWork = A.Fake<IUnitOfWork>();
            jobExecutor = A.Fake<IJobExecutor<MyJob>>();

            sut = new UnitOfWorkJobExecutor<MyJob>(unitOfWork, jobExecutor);
        }

        [Fact]
        public void BeginsAndCompletesUnitOfWork()
        {
            sut.ExecuteJob();
            A.CallTo(() => unitOfWork.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => unitOfWork.Complete()).MustHaveHappenedOnceExactly();
            A.CallTo(() => unitOfWork.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task BeginsAndCompletesUnitOfWorkAsync()
        {
            await sut.ExecuteJobAsync();
            A.CallTo(() => unitOfWork.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => unitOfWork.Complete()).MustHaveHappenedOnceExactly();
            A.CallTo(() => unitOfWork.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CallsJobExecutor()
        {
            sut.ExecuteJob();
            A.CallTo(() => jobExecutor.ExecuteJob()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task CallsJobExecutorAsync()
        {
            await sut.ExecuteJobAsync();
            A.CallTo(() => jobExecutor.ExecuteJobAsync()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void DoesNotCompleteUnitOfWorkOnException()
        {
            A.CallTo(() => jobExecutor.ExecuteJob()).Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(() => sut.ExecuteJob());
            A.CallTo(() => unitOfWork.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => unitOfWork.Dispose()).MustHaveHappenedOnceExactly();
            A.CallTo(() => unitOfWork.Complete()).MustNotHaveHappened();
        }

        [Fact]
        public async Task DoesNotCompleteUnitOfWorkOnExceptionAsync()
        {
            A.CallTo(() => jobExecutor.ExecuteJobAsync()).ThrowsAsync(new InvalidOperationException());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.ExecuteJobAsync());
            A.CallTo(() => unitOfWork.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => unitOfWork.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => unitOfWork.Dispose()).MustHaveHappenedOnceExactly();
            A.CallTo(() => unitOfWork.Complete()).MustNotHaveHappened();
        }
    }
}