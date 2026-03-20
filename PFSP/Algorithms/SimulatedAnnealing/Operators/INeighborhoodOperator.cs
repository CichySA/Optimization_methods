namespace PFSP.Algorithms.SimulatedAnnealing.Operators
{
    public interface INeighborhoodOperator
    {
        int[] CreateNeighbor(int[] permutation, Random rnd);
    }
}
