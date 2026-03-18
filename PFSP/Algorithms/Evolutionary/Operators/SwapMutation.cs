namespace PFSP.Algorithms.Evolutionary
{
    public sealed class SwapMutation : IMutationMethod
    {
        public void Mutate(int[] perm, System.Random rnd)
        {
            int n = perm.Length;
            if (n < 2) return;
            int i = rnd.Next(n);
            int j = rnd.Next(n - 1);
            if (j >= i) j++;
            (perm[i], perm[j]) = (perm[j], perm[i]);
        }
    }
}
