namespace PFSP.Algorithms.Evolutionary.Operators
{
    public interface ICrossoverMethod
    {
        int[] Crossover(int[] parent1, int[] parent2, Random rnd);
    }
}
