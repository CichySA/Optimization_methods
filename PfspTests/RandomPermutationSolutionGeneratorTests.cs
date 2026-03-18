using PFSP.Solutions.PermutationSolutionGenerators;

namespace PfspTests
{
    public class RandomPermutationSolutionGeneratorTests
    {
        [Fact]
        public void Shuffle_ProducesValidPermutation()
        {
            int n = 8;
            var gen = new RandomPermutationSolutionGenerator(seed: 42);
            int[] buffer = new int[n];

            gen.Shuffle(buffer);

            Assert.Equal(Enumerable.Range(0, n), buffer.OrderBy(x => x));
        }
    }
}
