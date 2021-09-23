using System.Linq;
using Backend.Fx.RandomData;
using Xunit;

namespace Backend.Fx.Tests.RandomData
{
    public class TheLandLineGenerator : TheGenerator<LandLineGenerator, string>
    { }


    public class TheMobileLineGenerator : TheGenerator<MobileLineGenerator, string>
    { }


    public class TheTestAddressGenerator : TheGenerator<TestAddressGenerator, TestAddress>
    { }


    public class TheTestPersonGenerator : TheGenerator<TestPersonGenerator, TestPerson>
    { }


    public class TheLoremIpsumGenerator : TheGenerator<LoremIpsumGenerator, string>
    {
        [Fact]
        public void GeneratesAsExpected()
        {
            string sentence = LoremIpsumGenerator.Generate(10, 10, true);
            Assert.Equal(10, sentence.Split(" ").Length);
            Assert.True(sentence.EndsWith('.'));

            sentence = LoremIpsumGenerator.Generate(10, 10, false);
            Assert.Equal(10, sentence.Split(" ").Length);
            Assert.False(sentence.EndsWith('.'));
        }
    }


    public abstract class TheGenerator<TGen, T> where TGen : Generator<T>, new()
    {
        private readonly TGen _sut;

        protected TheGenerator()
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
