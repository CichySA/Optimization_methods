namespace PFSP.Algorithms.Evolutionary.Operators
{
    public interface ICrossoverMethod
    {
        string Name { get; }
        int[] Crossover(int[] parent1, int[] parent2, Random rnd);
    }
}
