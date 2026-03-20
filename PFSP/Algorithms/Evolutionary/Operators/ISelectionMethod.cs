using PFSP.Solutions;

namespace PFSP.Algorithms.Evolutionary.Operators
{
    public interface ISelectionMethod
    {
        PermutationSolution Select(PermutationSolution[] population, Random rnd, int tournamentSize);
    }
}
