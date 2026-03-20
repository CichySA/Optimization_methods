using PFSP.Solutions;

namespace PFSP.Algorithms.Evolutionary.Operators
{
    /// <summary>
    /// Tournament selection method for evolutionary algorithms. Randomly selects a specified number of solutions from the population and returns the best one among them.
    /// </summary>
    public sealed class TournamentSelection : ISelectionMethod
    {
        /// <summary>
        /// Tournament selection using OneMax principle: randomly select a specified number of solutions from the population and return the best one among them.
        /// </summary>
        /// <param name="population"></param>
        /// <param name="rnd"></param>
        /// <param name="tournamentSize"></param>
        /// <returns></returns>
        public PermutationSolution Select(PermutationSolution[] population, Random rnd, int tournamentSize)
        {
            int n = population.Length;
            tournamentSize = Math.Max(1, Math.Min(n, tournamentSize));
            PermutationSolution best = null!;
            for (int i = 0; i < tournamentSize; i++)
            {
                var cand = population[rnd.Next(n)];
                if (best == null || cand.Cost < best.Cost) best = cand;
            }
            return best;
        }
    }
}
