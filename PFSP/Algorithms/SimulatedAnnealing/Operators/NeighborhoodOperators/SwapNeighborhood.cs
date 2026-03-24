using PFSP.Algorithms.SimulatedAnnealing.Operators;

namespace PFSP.Algorithms.SimulatedAnnealing.Operators.NeighborhoodOperators
{
    /// <summary>
    ///    The SwapNeighborhood operator creates a neighbor by selecting two positions in the permutation and swapping the elements at those positions.
    /// </summary>
    public sealed class SwapNeighborhood : INeighborhoodOperator
    {
        public const string Name = "Swap";
        string INeighborhoodOperator.Name => Name;

        public int[] CreateNeighbor(int[] permutation, Random rnd)
        {
            ArgumentNullException.ThrowIfNull(permutation);

            var neighbor = (int[])permutation.Clone();
            int n = neighbor.Length;
            if (n < 2) return neighbor;

            int i = rnd.Next(n);
            int j = rnd.Next(n - 1);
            if (j >= i) j++;
            (neighbor[i], neighbor[j]) = (neighbor[j], neighbor[i]);
            return neighbor;
        }
    }
}
