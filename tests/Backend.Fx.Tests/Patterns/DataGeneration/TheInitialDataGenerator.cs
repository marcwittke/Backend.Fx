namespace Backend.Fx.Tests.Patterns.DataGeneration
{
    using Fx.Patterns.DataGeneration;
    using Xunit;

    public class AnInitialDataGenerator : InitialDataGenerator
    {

        public bool ShouldRunOverride { get; set; }
        public int GenerateCoreCalled { get; private set; }
        public int ShouldRunCalled { get; private set; }
        public int InitializeCalled { get; private set; }

        public override int Priority { get { return 12; } }

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

    public class TheInitialDataGenerator
    {
        private readonly AnInitialDataGenerator _sut = new AnInitialDataGenerator();

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
    }
}
