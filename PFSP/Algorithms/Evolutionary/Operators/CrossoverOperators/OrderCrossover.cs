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
        /// Creates a new offspring array by performing OX between two parents
        /// </summary>
        public int[] Crossover(int[] parent1, int[] parent2, Random rnd)
        {
            int n = parent1.Length;
            var child = new int[n];
            for (int i = 0; i < n; i++) child[i] = -1;

            int a = rnd.Next(n);
            int b = rnd.Next(n);
            if (a > b) (a, b) = (b, a);

            Array.Copy(parent1, a, child, a, b - a + 1);

            int idx = (b + 1) % n;
            for (int i = 0; i < n; i++)
            {
                int v = parent2[(b + 1 + i) % n];
                bool present = false;
                for (int j = a; j <= b; j++) if (child[j] == v) { present = true; break; }
                if (present) continue;
                child[idx] = v;
                idx = (idx + 1) % n;
            }
            return child;
        }
    }
}
