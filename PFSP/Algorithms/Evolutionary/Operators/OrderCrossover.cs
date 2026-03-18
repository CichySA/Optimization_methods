namespace PFSP.Algorithms.Evolutionary
{
    public sealed class OrderCrossover : ICrossoverMethod
    {
        public int[] Crossover(int[] p1, int[] p2, System.Random rnd)
        {
            int n = p1.Length;
            var child = new int[n];
            for (int i = 0; i < n; i++) child[i] = -1;

            int a = rnd.Next(n);
            int b = rnd.Next(n);
            if (a > b) (a, b) = (b, a);

            for (int i = a; i <= b; i++) child[i] = p1[i];

            int idx = (b + 1) % n;
            for (int i = 0; i < n; i++)
            {
                int v = p2[(b + 1 + i) % n];
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
