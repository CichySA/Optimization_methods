namespace PFSP.Algorithms.SimulatedAnnealing.Operators
{
    public interface INeighborhoodOperator
    {
        string Name { get; }
        int[] CreateNeighbor(int[] permutation, Random rnd);
    }
}
