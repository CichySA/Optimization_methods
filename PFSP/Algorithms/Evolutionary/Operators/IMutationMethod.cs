namespace PFSP.Algorithms.Evolutionary.Operators
{
    public interface IMutationMethod
    {
        void Mutate(int[] permutation, Random rnd);
    }
}
