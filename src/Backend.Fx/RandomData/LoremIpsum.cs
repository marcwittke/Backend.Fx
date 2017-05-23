namespace Backend.Fx.RandomData
{
    public class LoremIpsum
    {
        public static string Generate(int minWords, int maxWords, bool asSentence)
        {
            var words = new[] {
                "lorem",
                "ipsum",
                "dolor",
                "sit",
                "amet",
                "consectetuer",
                "adipiscing",
                "elit",
                "sed",
                "diam",
                "nonummy",
                "nibh",
                "euismod",
                "tincidunt",
                "ut",
                "laoreet",
                "dolore",
                "magna",
                "aliquam",
                "erat"
            };

            var numWords = TestRandom.Instance.Next(maxWords - minWords) + minWords + 1;

            var result = string.Empty;

            for (var w = 0; w < numWords; w++)
            {
                if (w > 0)
                {
                    result += " ";
                }
                result += words[TestRandom.Instance.Next(words.Length)];
            }

            if (asSentence)
            {
                result += ". ";
            }
            return result;
        }
    }
}