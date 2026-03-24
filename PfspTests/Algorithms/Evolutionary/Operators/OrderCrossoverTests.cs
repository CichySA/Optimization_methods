using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.Evolutionary.Operators.CrossoverOperators;

namespace PfspTests.Algorithms.Evolutionary.Operators
{
    public class OrderCrossoverTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        [InlineData(123)]
        public void Crossover_BothChildrenAreValidPermutations(int seed)
        {
            var ox = new OrderCrossover();
            var parent1 = new[] { 0, 1, 2, 3, 4, 5 };
            var parent2 = new[] { 3, 4, 5, 0, 1, 2 };

            var (child1, child2) = ox.Crossover(parent1, parent2, new Random(seed));

            Assert.Equal(Enumerable.Range(0, 6), child1.OrderBy(x => x));
            Assert.Equal(Enumerable.Range(0, 6), child2.OrderBy(x => x));
        }

        [Fact]
        public void Crossover_SegmentIsPreservedInBothChildren()
        {
            // Use parents where no position has the same value in both, so the positions
            // where child1[i]==parent1[i] are exactly the inherited segment positions.
            // Both children must inherit the same contiguous segment (from opposite parents).
            var ox = new OrderCrossover();
            var parent1 = new[] { 0, 1, 2, 3, 4, 5 };
            var parent2 = new[] { 3, 4, 5, 0, 1, 2 };

            foreach (int seed in new[] { 0, 7, 42, 99, 200 })
            {
                var (child1, child2) = ox.Crossover(parent1, parent2, new Random(seed));

                var segmentPositions1 = Enumerable.Range(0, 6).Where(i => child1[i] == parent1[i]).ToList();
                var segmentPositions2 = Enumerable.Range(0, 6).Where(i => child2[i] == parent2[i]).ToList();

                Assert.Equal(segmentPositions1, segmentPositions2);
                for (int k = 0; k < segmentPositions1.Count - 1; k++)
                    Assert.Equal(segmentPositions1[k] + 1, segmentPositions1[k + 1]);
            }
        }

        [Fact]
        public void Crossover_ValuesOutsideSegmentComeFromOppositeParent()
        {
            // The values placed in child1's non-segment positions are exactly the values from
            // parent2 that are not present in child1's inherited segment.
            var ox = new OrderCrossover();
            var parent1 = new[] { 0, 1, 2, 3, 4, 5 };
            var parent2 = new[] { 3, 4, 5, 0, 1, 2 };

            var (child1, _) = ox.Crossover(parent1, parent2, new Random(42));

            var segmentPositions = Enumerable.Range(0, 6).Where(i => child1[i] == parent1[i]).ToHashSet();
            var segmentValues = segmentPositions.Select(i => child1[i]).ToHashSet();

            var nonSegmentValuesChild1 = Enumerable.Range(0, 6)
                .Where(i => !segmentPositions.Contains(i))
                .Select(i => child1[i])
                .ToHashSet();
            var parent2ValuesOutsideSegment = parent2.Where(v => !segmentValues.Contains(v)).ToHashSet();

            Assert.Equal(parent2ValuesOutsideSegment, nonSegmentValuesChild1);
        }

        [Fact]
        public void Name_IsRegisteredInCrossoverRegistry()
        {
            Assert.Contains(OrderCrossover.Name, EvolutionaryParameterFactory.CrossoverRegistry.Keys);
        }
    }
}
