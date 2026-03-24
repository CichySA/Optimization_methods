using PFSP.Algorithms.Evolutionary.Operators;

namespace PFSP.Algorithms.Evolutionary.Operators.CrossoverOperators
{
    /// <summary>
    /// Implements the order crossover (OX) genetic algorithm operator for permutations.
    /// </summary>
    public sealed class OrderCrossover : ICrossoverMethod
    {
        public const string Name = "OX";
        string ICrossoverMethod.Name => Name;

        /// <summary>
        /// Produces both OX children using the same random segment.
        /// Child1 copies segment [a,b] from parent1 and fills gaps from parent2; child2 does the reverse.
        /// </summary>
        public (int[] Child1, int[] Child2) Crossover(int[] parent1, int[] parent2, Random rnd)
        {
            int n = parent1.Length;
            var child1 = new int[n];
            var child2 = new int[n];
            for (int i = 0; i < n; i++) { child1[i] = -1; child2[i] = -1; }

            int a = rnd.Next(n);
            int b = rnd.Next(n);
            if (a > b) (a, b) = (b, a);

            Array.Copy(parent1, a, child1, a, b - a + 1);
            Array.Copy(parent2, a, child2, a, b - a + 1);

            // Modulo to skip the segment and wrap around the end of the array
            int idx1 = (b + 1) % n;
            int idx2 = (b + 1) % n;
            for (int i = 0; i < n; i++)
            {
                int v1 = parent2[(b + 1 + i) % n];
                bool present1 = false;
                for (int j = a; j <= b; j++) if (child1[j] == v1) { present1 = true; break; }
                if (!present1) { child1[idx1] = v1; idx1 = (idx1 + 1) % n; }

                int v2 = parent1[(b + 1 + i) % n];
                bool present2 = false;
                for (int j = a; j <= b; j++) if (child2[j] == v2) { present2 = true; break; }
                if (!present2) { child2[idx2] = v2; idx2 = (idx2 + 1) % n; }
            }

            return (child1, child2);
        }
    }
}
