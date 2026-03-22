using PFSP.Algorithms.Evolutionary.Operators;
using PFSP.Solutions;

namespace PFSP.Algorithms.Evolutionary.Operators.SelectionOperators
{
    /// <summary>
    /// Tournament selection method for evolutionary algorithms. Randomly selects a specified number of solutions from the population and returns the best one among them.
    /// </summary>
    public sealed class TournamentSelection : ISelectionMethod
    {
        /// <summary>
        /// Tournament selection using OneMax principle: randomly select a specified number of solutions from the population and return the best one among them.
        /// </summary>
        public PermutationSolution Select(PermutationSolution[] population, Random rnd, ISelectionParameters parameters)
        {
            if (parameters is not TournamentSelectionParameters selection)
                throw new ArgumentException($"{nameof(TournamentSelection)} requires {nameof(TournamentSelectionParameters)}.");

            int n = population.Length;
            var tournamentSize = selection.TournamentSize;
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
