namespace PFSP.Algorithms.Evolutionary.Operators.MutationOperators
{
    public sealed class InsertMutation : IMutationMethod
    {
        public const string Name = "Insert";
        string IMutationMethod.Name => Name;

        /// <summary>
        /// Removes the element at a randomly selected position and reinserts it at another random position.
        /// </summary>
        public void Mutate(int[] permutation, Random rnd)
        {
            int n = permutation.Length;
            if (n < 2) return;
            int i = rnd.Next(n);
            int j = rnd.Next(n - 1);
            if (j >= i) j++;

            int val = permutation[i];
            if (i < j)
                Array.Copy(permutation, i + 1, permutation, i, j - i);
            else
                Array.Copy(permutation, j, permutation, j + 1, i - j);
            permutation[j] = val;
        }
    }
}
