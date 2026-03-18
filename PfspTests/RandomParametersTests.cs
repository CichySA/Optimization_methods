using PFSP.Algorithms.Random;

namespace PfspTests
{
    public class RandomParametersTests
    {
        [Fact]
        public void ForRuns_ZeroSamples_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => RandomParameters.ForRuns(samples: 0));
        }

        [Fact]
        public void ForRuns_NegativeSeed_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => RandomParameters.ForRuns(samples: 10, seed: -1));
        }
    }
}
