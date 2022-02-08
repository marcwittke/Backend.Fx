using System.Linq;
using Backend.Fx.RandomData;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.RandomData
{
    public class TheLandLineGenerator : TheGenerator<LandLineGenerator, string>
    {
        public TheLandLineGenerator(ITestOutputHelper output) : base(output)
        {
        }
    }
    public class TheMobileLineGenerator : TheGenerator<MobileLineGenerator, string>
    {
        public TheMobileLineGenerator(ITestOutputHelper output) : base(output)
        {
        }
    }
    public class TheTestAddressGenerator : TheGenerator<TestAddressGenerator, TestAddress>
    {
        public TheTestAddressGenerator(ITestOutputHelper output) : base(output)
        {
        }
    }
    public class TheTestPersonGenerator : TheGenerator<TestPersonGenerator, TestPerson>
    {
        public TheTestPersonGenerator(ITestOutputHelper output) : base(output)
        {
        }
    }

    public class TheLoremIpsumGenerator : TheGenerator<LoremIpsumGenerator, string>
    {
        [Fact]
        public void GeneratesAsExpected()
        {
            var sentence = LoremIpsumGenerator.Generate(10, 10, true);
            Assert.Equal(10, sentence.Split(" ").Length);
            Assert.True(sentence.EndsWith('.'));
            
            sentence = LoremIpsumGenerator.Generate(10, 10, false);
            Assert.Equal(10, sentence.Split(" ").Length);
            Assert.False(sentence.EndsWith('.'));
        }

        public TheLoremIpsumGenerator(ITestOutputHelper output) : base(output)
        {
        }
    }
    
    public abstract class TheGenerator<TGen, T> : TestWithLogging where TGen : Generator<T>, new()
    {
        private readonly TGen _sut;

        protected TheGenerator(ITestOutputHelper output): base(output)
        {
            _sut = new TGen();
        }

        [Fact]
        public void CanGenerateMany()
        {
            T[] generated = _sut.Take(100).ToArray();
            Assert.Equal(100, generated.Length);
            Assert.Equal(100, generated.Distinct().Count());
        }
        
        [Fact]
        public void GeneratesNotEqual()
        {
            T[] generated = _sut.Take(100).ToArray();
            Assert.Equal(100, generated.Distinct().Count());
        }
    }
}