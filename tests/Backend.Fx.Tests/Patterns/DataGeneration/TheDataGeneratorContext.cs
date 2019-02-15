using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.UnitOfWork;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DataGeneration
{
    public class TheDataGeneratorContext
    {
        private readonly DataGeneratorContext _sut;
        private readonly IDemoDataGenerator _demoDataGenerator;
        private readonly IProductiveDataGenerator _productiveDataGenerator;
        private readonly ICanFlush _canFlush;

        public TheDataGeneratorContext()
        {
            _demoDataGenerator = A.Fake<IDemoDataGenerator>();
            _productiveDataGenerator = A.Fake<IProductiveDataGenerator>();
            _canFlush = A.Fake<ICanFlush>();
            _sut = new DataGeneratorContext(new IDataGenerator[] { _demoDataGenerator, _productiveDataGenerator }, _canFlush);
        }

        [Fact]
        public void RunsOnlyProductiveDataGenerators()
        {
            _sut.RunProductiveDataGenerators();

            A.CallTo(() => _productiveDataGenerator.Generate()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _demoDataGenerator.Generate()).MustNotHaveHappened();
            A.CallTo(() => _canFlush.Flush()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void RunsOnlyDemoDataGenerators()
        {
            _sut.RunDemoDataGenerators();
            A.CallTo(() => _productiveDataGenerator.Generate()).MustNotHaveHappened();
            A.CallTo(() => _demoDataGenerator.Generate()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _canFlush.Flush()).MustHaveHappenedOnceExactly();
        }
    }
}
