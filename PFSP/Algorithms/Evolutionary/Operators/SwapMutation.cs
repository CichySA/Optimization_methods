namespace PFSP.Algorithms.Evolutionary.Operators
{
    public sealed class SwapMutation : IMutationMethod
    {
        /// <summary>
        /// swap two randomly selected positions in the permutation
        /// </summary>
        public void Mutate(int[] permutation, Random rnd)
        {
            int n = permutation.Length;
            if (n < 2) return;
            int i = rnd.Next(n);
            int j = rnd.Next(n - 1);
            if (j >= i) j++;
            (permutation[i], permutation[j]) = (permutation[j], permutation[i]);
        }
    }
}
