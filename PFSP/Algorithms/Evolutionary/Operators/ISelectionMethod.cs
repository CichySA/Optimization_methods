using PFSP.Solutions;

namespace PFSP.Algorithms.Evolutionary
{
    public interface ISelectionMethod
    {
        PermutationSolution Select(PermutationSolution[] population, System.Random rnd, int tournamentSize);
    }
}
