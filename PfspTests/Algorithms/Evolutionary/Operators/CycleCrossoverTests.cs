using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.Evolutionary.Operators.CrossoverOperators;

namespace PfspTests.Algorithms.Evolutionary.Operators
{
    public class CycleCrossoverTests
    {
        private static readonly Random IgnoredRnd = new(0); // CX does not use rnd

        [Fact]
        public void Crossover_BothChildrenAreValidPermutations()
        {
            var cx = new CycleCrossover();
            var parent1 = new[] { 0, 1, 2, 3, 4 };
            var parent2 = new[] { 1, 2, 0, 4, 3 };

            var (child1, child2) = cx.Crossover(parent1, parent2, IgnoredRnd);

            Assert.Equal(Enumerable.Range(0, 5), child1.OrderBy(x => x));
            Assert.Equal(Enumerable.Range(0, 5), child2.OrderBy(x => x));
        }

        [Fact]
        public void Crossover_KnownExample_ProducesCorrectChildren()
        {
            // parent1 = [0,1,2,3,4], parent2 = [1,2,0,4,3]
            // Cycle 1 (odd)  → positions {0,1,2} from parent1
            // Cycle 2 (even) → positions {3,4}   from parent2
            var cx = new CycleCrossover();
            var parent1 = new[] { 0, 1, 2, 3, 4 };
            var parent2 = new[] { 1, 2, 0, 4, 3 };

            var (child1, child2) = cx.Crossover(parent1, parent2, IgnoredRnd);

            Assert.Equal(new[] { 0, 1, 2, 4, 3 }, child1);
            Assert.Equal(new[] { 1, 2, 0, 3, 4 }, child2);
        }

        [Fact]
        public void Crossover_ComplementProperty_EachPositionTakenFromDifferentParents()
        {
            // At every position exactly one child inherits from parent1 and the other from parent2.
            var cx = new CycleCrossover();
            var parent1 = new[] { 0, 1, 2, 3, 4, 5 };
            var parent2 = new[] { 2, 3, 4, 5, 0, 1 };

            var (child1, child2) = cx.Crossover(parent1, parent2, IgnoredRnd);

            for (int pos = 0; pos < parent1.Length; pos++)
            {
                bool child1FromP1 = child1[pos] == parent1[pos];
                bool child1FromP2 = child1[pos] == parent2[pos];
                Assert.True(child1FromP1 || child1FromP2,
                    $"child1[{pos}]={child1[pos]} matches neither parent1[{pos}]={parent1[pos]} nor parent2[{pos}]={parent2[pos]}");

                if (child1FromP1)
                    Assert.Equal(parent2[pos], child2[pos]);
                else
                    Assert.Equal(parent1[pos], child2[pos]);
            }
        }

        [Fact]
        public void Name_IsRegisteredInCrossoverRegistry()
        {
            Assert.Contains(CycleCrossover.Name, EvolutionaryParameterFactory.CrossoverRegistry.Keys);
        }
    }
}
