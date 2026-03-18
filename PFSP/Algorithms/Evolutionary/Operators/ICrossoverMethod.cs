namespace PFSP.Algorithms.Evolutionary
{
    public interface ICrossoverMethod
    {
        int[] Crossover(int[] parent1, int[] parent2, System.Random rnd);
    }
}
