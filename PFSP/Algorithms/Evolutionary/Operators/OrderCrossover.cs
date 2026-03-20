namespace PFSP.Algorithms.Evolutionary.Operators
{
    /// <summary>
    /// Implements the order crossover (OX) genetic algorithm operator for permutations.
    /// </summary>
    public sealed class OrderCrossover : ICrossoverMethod
    {
        /// <summary>
        /// Creates a new offspring array by performing OX between two parents
        /// </summary>
        /// <remarks>The method assumes that both parent arrays are of equal length and represent valid
        /// permutations.</remarks>
        /// <param name="parent1">First parent array.</param>
        /// <param name="parent2">The second parent array.</param>
        /// <param name="rnd"></returns>
        public int[] Crossover(int[] parent1, int[] parent2, Random rnd)
        {
            int n = parent1.Length;
            var child = new int[n];
            // To avoid  errors in child[j] = v; when child[j] is not initialized yet
            for (int i = 0; i < n; i++) child[i] = -1;

            int a = rnd.Next(n);
            int b = rnd.Next(n);
            if (a > b) (a, b) = (b, a);

            // Copy the segment from parent 1 to child
            Array.Copy(parent1, a, child, a, b - a + 1);

            // idx translates the index in order to avoid overwriting the segment copied from parent 1. It starts from b + 1 and wraps around using modulo
            int idx = (b + 1) % n;
            // insert all missing values from parent 2 in order
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
