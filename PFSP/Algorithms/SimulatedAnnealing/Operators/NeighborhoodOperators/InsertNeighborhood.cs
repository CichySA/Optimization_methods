using PFSP.Algorithms.SimulatedAnnealing.Operators;

namespace PFSP.Algorithms.SimulatedAnnealing.Operators.NeighborhoodOperators
{
    /// <summary>
    ///   The InsertNeighborhood operator creates a neighbor by removing an element from the permutation and inserting it at a different position.
    /// </summary>
    public sealed class InsertNeighborhood : INeighborhoodOperator
    {
        public const string Name = "Insert";
        string INeighborhoodOperator.Name => Name;

        public int[] CreateNeighbor(int[] permutation, Random rnd)
        {
            ArgumentNullException.ThrowIfNull(permutation);

            int n = permutation.Length;
            if (n < 2) return (int[])permutation.Clone();

            var neighbor = (int[])permutation.Clone();
            int from = rnd.Next(n);
            int to = rnd.Next(n - 1);
            if (to >= from) to++;

            int value = neighbor[from];
            if (from < to)
            {
                Array.Copy(neighbor, from + 1, neighbor, from, to - from);
            }
            else if (from > to)
            {
                Array.Copy(neighbor, to, neighbor, to + 1, from - to);
            }

            neighbor[to] = value;

            return neighbor;
        }
    }
}
