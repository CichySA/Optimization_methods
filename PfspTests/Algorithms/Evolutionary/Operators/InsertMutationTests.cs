using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.Evolutionary.Operators.MutationOperators;

namespace PfspTests.Algorithms.Evolutionary.Operators
{
    public class InsertMutationTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        [InlineData(123)]
        public void Mutate_ProducesValidPermutation(int seed)
        {
            var mutation = new InsertMutation();
            var permutation = new[] { 0, 1, 2, 3, 4, 5, 6 };

            mutation.Mutate(permutation, new Random(seed));

            Assert.Equal(Enumerable.Range(0, 7), permutation.OrderBy(x => x));
        }

        [Fact]
        public void Mutate_ExactlyOneElementDisplaced()
        {
            // Removing the moved value from both original and mutated must yield equal sequences,
            // because insert mutation preserves the relative order of every other element.
            var mutation = new InsertMutation();
            var original = new[] { 0, 1, 2, 3, 4 };
            var permutation = (int[])original.Clone();

            mutation.Mutate(permutation, new Random(99));

            bool found = false;
            for (int v = 0; v < original.Length; v++)
            {
                if (original.Where(x => x != v).SequenceEqual(permutation.Where(x => x != v)))
                {
                    found = true;
                    break;
                }
            }
            Assert.True(found, "No single removed value makes the original and mutated sequences equal.");
        }

        [Fact]
        public void Mutate_SingleElement_LeavesPermutationUnchanged()
        {
            var mutation = new InsertMutation();
            var permutation = new[] { 7 };

            mutation.Mutate(permutation, new Random(0));

            Assert.Equal(7, permutation[0]);
        }

        [Fact]
        public void Mutate_TwoElements_AlwaysSwaps()
        {
            // For n=2: i is 0 or 1, j=rnd.Next(1)=0, the j>=i guard always makes j=the other index.
            var mutation = new InsertMutation();
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
            Assert.Contains(InsertMutation.Name, EvolutionaryParameterFactory.MutationRegistry.Keys);
        }
    }
}
