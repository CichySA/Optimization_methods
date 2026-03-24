using PFSP.Solutions;

namespace PFSP.Algorithms.Evolutionary.Operators
{
    public interface ISelectionMethod
    {
        string Name { get; }
        PermutationSolution Select(PermutationSolution[] population, Random rnd, ISelectionParameters parameters);
    }
}
