namespace Backend.Fx.Tests.Patterns.DataGeneration
{
    using FakeItEasy;
    using Fx.Patterns.DataGeneration;
    using Xunit;

    public class AnInitialDataGenerator : InitialDataGenerator {

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
        private readonly AnInitialDataGenerator sut = new AnInitialDataGenerator();

        [Fact]
        public void RespectsNegativeShouldRunMethodResult()
        {
            sut.Generate();
            Assert.Equal(1, sut.ShouldRunCalled);
            Assert.Equal(0, sut.GenerateCoreCalled);
            Assert.Equal(0, sut.InitializeCalled);
        }

        [Fact]
        public void RespectsPositiveShouldRunMethodResult()
        {
            sut.ShouldRunOverride = true;
            sut.Generate();
            Assert.Equal(1, sut.ShouldRunCalled);
            Assert.Equal(1, sut.GenerateCoreCalled);
            Assert.Equal(1, sut.InitializeCalled);
        }
    }
}
