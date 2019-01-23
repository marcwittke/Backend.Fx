using Xunit;

namespace Backend.Fx.Tests.Patterns.Jobs
{
    using System;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Fx.Patterns.Jobs;
    using Fx.Patterns.UnitOfWork;
    using JetBrains.Annotations;

    public class TheUnitOfWorkJobExecutor
    {
        [UsedImplicitly]
        public class MyJob : IJob
        {
            public void Run()
            { }
        }

        private readonly UnitOfWorkJobExecutor<MyJob> _sut;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJobExecutor<MyJob> _jobExecutor;

        public TheUnitOfWorkJobExecutor()
        {
            _unitOfWork = A.Fake<IUnitOfWork>();
            _jobExecutor = A.Fake<IJobExecutor<MyJob>>();

            _sut = new UnitOfWorkJobExecutor<MyJob>(_unitOfWork, _jobExecutor);
        }

        [Fact]
        public void BeginsAndCompletesUnitOfWork()
        {
            _sut.ExecuteJob();
            A.CallTo(() => _unitOfWork.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.Complete()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CallsJobExecutor()
        {
            _sut.ExecuteJob();
            A.CallTo(() => _jobExecutor.ExecuteJob()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void DoesNotCompleteUnitOfWorkOnException()
        {
            A.CallTo(() => _jobExecutor.ExecuteJob()).Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(() => _sut.ExecuteJob());
            A.CallTo(() => _unitOfWork.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.Dispose()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.Complete()).MustNotHaveHappened();
        }
    }
}