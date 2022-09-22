using Backend.Fx.Extensions.DataGeneration;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.DataGeneration
{
    public class ADataGenerator : DataGenerator
    {
        public bool ShouldRunOverride { get; set; }
        public int GenerateCoreCalled { get; private set; }
        public int ShouldRunCalled { get; private set; }
        public int InitializeCalled { get; private set; }

        public override int Priority => 12;

        protected override void GenerateCore()
        {
            GenerateCoreCalled++;
        }

        protected override void Initialize()
        {
            InitializeCalled++;
        }

        protected override bool ShouldRun()
        {
            ShouldRunCalled++;
            return ShouldRunOverride;
        }
    }

    public class TheInitialDataGenerator : TestWithLogging
    {
        private readonly ADataGenerator _sut = new ADataGenerator();

        [Fact]
        public void RespectsNegativeShouldRunMethodResult()
        {
            _sut.Generate();
            Assert.Equal(1, _sut.ShouldRunCalled);
            Assert.Equal(0, _sut.GenerateCoreCalled);
            Assert.Equal(0, _sut.InitializeCalled);
        }

        [Fact]
        public void RespectsPositiveShouldRunMethodResult()
        {
            _sut.ShouldRunOverride = true;
            _sut.Generate();
            Assert.Equal(1, _sut.ShouldRunCalled);
            Assert.Equal(1, _sut.GenerateCoreCalled);
            Assert.Equal(1, _sut.InitializeCalled);
        }

        public TheInitialDataGenerator(ITestOutputHelper output) : base(output)
        {
        }
    }
}