namespace PFSP.Algorithms.Evolutionary.Operators
{
    public interface IMutationMethod
    {
        string Name { get; }
        void Mutate(int[] permutation, Random rnd);
    }
}
