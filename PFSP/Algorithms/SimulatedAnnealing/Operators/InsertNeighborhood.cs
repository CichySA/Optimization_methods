namespace PFSP.Algorithms.SimulatedAnnealing.Operators
{
    /// <summary>
    ///   The InsertNeighborhood operator creates a neighbor by removing an element from the permutation and inserting it at a different position.
    /// </summary>
    public sealed class InsertNeighborhood : INeighborhoodOperator
    {
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
            // Shift elements to make room for the inserted value
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
