using PFSP.Algorithms.Evolutionary.Operators;

namespace PFSP.Algorithms.Evolutionary.Operators.CrossoverOperators
{
    /// <summary>
    /// Implements the Cycle Crossover (CX) operator for permutations.
    /// </summary>
    public sealed class CycleCrossover : ICrossoverMethod
    {
        public const string Name = "CX";
        string ICrossoverMethod.Name => Name;

        /// <summary>
        /// Child1 takes odd cycles from parent1, child2 from parent2; even cycles are the reverse.
        /// </summary>
        public (int[] Child1, int[] Child2) Crossover(int[] parent1, int[] parent2, Random rnd)
        {
            int n = parent1.Length;
            var child1 = new int[n];
            var child2 = new int[n];

            Span<int> pos1 = stackalloc int[n];
            for (int i = 0; i < n; i++)
                pos1[parent1[i]] = i;

            Span<bool> visited = stackalloc bool[n];
            int cycleNumber = 0;

            for (int start = 0; start < n; start++)
            {
                if (visited[start]) continue;

                cycleNumber++;
                bool fromParent1 = cycleNumber % 2 == 1;
                int pos = start;
                while (!visited[pos])
                {
                    visited[pos] = true;
                    child1[pos] = fromParent1 ? parent1[pos] : parent2[pos];
                    child2[pos] = fromParent1 ? parent2[pos] : parent1[pos];
                    pos = pos1[parent2[pos]];
                }
            }

            return (child1, child2);
        }
    }
}
