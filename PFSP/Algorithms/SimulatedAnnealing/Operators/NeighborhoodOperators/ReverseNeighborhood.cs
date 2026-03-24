using PFSP.Algorithms.SimulatedAnnealing.Operators;

namespace PFSP.Algorithms.SimulatedAnnealing.Operators.NeighborhoodOperators
{
    /// <summary>
    ///    The ReverseNeighborhood operator creates a neighbor by selecting two positions in the permutation and reversing the order of the elements between those positions.
    /// </summary>
    public sealed class ReverseNeighborhood : INeighborhoodOperator
    {
        public const string Name = "Reverse";
        string INeighborhoodOperator.Name => Name;

        public int[] CreateNeighbor(int[] permutation, Random rnd)
        {
            ArgumentNullException.ThrowIfNull(permutation);

            var neighbor = (int[])permutation.Clone();
            int n = neighbor.Length;
            if (n < 2) return neighbor;

            int a = rnd.Next(n);
            int b = rnd.Next(n);
            if (a > b) (a, b) = (b, a);

            Array.Reverse(neighbor, a, b - a + 1);
            return neighbor;
        }
    }
}
