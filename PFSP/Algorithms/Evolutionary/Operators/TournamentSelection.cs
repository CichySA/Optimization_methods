using System;
using PFSP.Solutions;

namespace PFSP.Algorithms.Evolutionary
{
    public sealed class TournamentSelection : ISelectionMethod
    {
        public PermutationSolution Select(PermutationSolution[] population, System.Random rnd, int tournamentSize)
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
