namespace PFSP.Algorithms.Evolutionary
{
    public interface IMutationMethod
    {
        void Mutate(int[] permutation, System.Random rnd);
    }
}
