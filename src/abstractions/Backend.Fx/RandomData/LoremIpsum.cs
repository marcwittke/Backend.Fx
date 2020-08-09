using System.Linq;

namespace Backend.Fx.RandomData
{
    public class LoremIpsumGenerator : Generator<string>
    {
        private static string[] _words = new[]
        {
            "lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing", "elit", "proin", "eget", "iaculis", "quam", "pellentesque", "elementum", "gravida", "nulla",
            "at", "tincidunt", "donec", "vulputate", "velit", "sapien", "a", "auctor", "justo", "id", "nunc", "et", "consequat", "magna", "in", "blandit", "ut", "eros",
            "tempus", "condimentum", "sem", "ac", "feugiat", "tellus", "curabitur", "aliquet", "ultrices", "arcu", "eu", "lacinia", "aliquam", "integer", "non", "venenatis",
            "sed", "accumsan", "massa", "nibh", "vestibulum", "nec", "porta", "libero", "vel", "ex", "molestie", "pretium", "dignissim", "ligula", "maximus", "placerat",
            "nisl", "felis", "fringilla", "efficitur", "mi", "nam", "vitae", "orci", "suscipit", "porttitor", "leo", "posuere", "sollicitudin", "dictum", "tristique", "dui",
            "urna", "quis", "quisque", "semper", "diam", "pulvinar", "erat", "ornare", "maecenas", "euismod", "odio", "tortor", "cursus", "convallis", "enim", "sodales",
            "facilisis", "faucibus", "fusce", "scelerisque", "purus", "praesent", "interdum", "turpis", "mauris", "duis", "finibus", "augue", "nullam", "mollis", "lacus",
            "egestas", "metus", "mattis", "morbi", "laoreet", "bibendum", "phasellus", "risus", "neque", "volutpat", "lobortis", "malesuada", "sagittis", "rhoncus", "est",
            "imperdiet", "aenean", "fermentum", "varius", "vivamus", "suspendisse", "commodo", "luctus", "dapibus", "ullamcorper", "viverra", "congue", "hendrerit", "pharetra",
            "tempor", "eleifend", "lectus", "te"
        };

        protected override string Next()
        {
            return _words[TestRandom.Instance.Next(_words.Length)];
        }

        public static string Generate(int minWords, int maxWords, bool asSentence)
        {
            int wordCount = TestRandom.Next(minWords, maxWords);
            string loremIpsumText = string.Join(" ", new LoremIpsumGenerator().Take(wordCount));
            if (asSentence)
            {
                loremIpsumText += ".";
            }

            return loremIpsumText;
        } 
    }
}