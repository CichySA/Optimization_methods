using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.Evolutionary.Operators.MutationOperators;

namespace PfspTests.Algorithms.Evolutionary.Operators
{
    public class SwapMutationTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        [InlineData(123)]
        public void Mutate_ProducesValidPermutation(int seed)
        {
            var mutation = new SwapMutation();
            var permutation = new[] { 0, 1, 2, 3, 4, 5, 6 };

            mutation.Mutate(permutation, new Random(seed));

            Assert.Equal(Enumerable.Range(0, 7), permutation.OrderBy(x => x));
        }

        [Fact]
        public void Mutate_ExactlyTwoPositionsAreSwapped()
        {
            // After a swap mutation exactly two positions differ from the original
            // and the values at those positions are exchanged.
            var mutation = new SwapMutation();
            var original = new[] { 0, 1, 2, 3, 4 };
            var permutation = (int[])original.Clone();

            mutation.Mutate(permutation, new Random(99));

            var diffPositions = Enumerable.Range(0, original.Length)
                .Where(i => original[i] != permutation[i])
                .ToList();
            Assert.Equal(2, diffPositions.Count);
            Assert.Equal(original[diffPositions[0]], permutation[diffPositions[1]]);
            Assert.Equal(original[diffPositions[1]], permutation[diffPositions[0]]);
        }

        [Fact]
        public void Mutate_SingleElement_LeavesPermutationUnchanged()
        {
            var mutation = new SwapMutation();
            var permutation = new[] { 7 };

            mutation.Mutate(permutation, new Random(0));

            Assert.Equal(7, permutation[0]);
        }

        [Fact]
        public void Mutate_TwoElements_AlwaysSwaps()
        {
            // For n=2: i is 0 or 1, j=rnd.Next(1)=0, the j>=i guard always yields j=the other index.
            var mutation = new SwapMutation();
            foreach (int seed in new[] { 0, 1, 7, 42, 99 })
            {
                var permutation = new[] { 10, 20 };
                mutation.Mutate(permutation, new Random(seed));
                Assert.Equal(new[] { 20, 10 }, permutation);
            }
        }

        [Fact]
        public void Name_IsRegisteredInMutationRegistry()
        {
            Assert.Contains(SwapMutation.Name, EvolutionaryParameterFactory.MutationRegistry.Keys);
        }
    }
}
