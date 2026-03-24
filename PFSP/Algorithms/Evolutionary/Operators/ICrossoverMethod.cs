namespace PFSP.Algorithms.Evolutionary.Operators
{
    public interface ICrossoverMethod
    {
        string Name { get; }
        (int[] Child1, int[] Child2) Crossover(int[] parent1, int[] parent2, Random rnd);
    }
}
