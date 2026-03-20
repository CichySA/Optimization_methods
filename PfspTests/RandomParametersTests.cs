using PFSP.Algorithms.RandomSearch;

namespace PfspTests
{
    public class RandomParametersTests
    {
        [Fact]
        public void ForRuns_ZeroSamples_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => RandomSearchParameters.ForRuns(samples: 0));
        }

        [Fact]
        public void ForRuns_NegativeSeed_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => RandomSearchParameters.ForRuns(samples: 10, seed: -1));
        }
    }
}
