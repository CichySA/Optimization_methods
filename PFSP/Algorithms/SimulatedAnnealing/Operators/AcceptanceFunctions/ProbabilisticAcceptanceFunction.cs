using PFSP.Algorithms.SimulatedAnnealing.Operators;

namespace PFSP.Algorithms.SimulatedAnnealing.Operators.AcceptanceFunctions
{
    /// <summary>
    ///     based on Kirkpatrick et al. (1983)
    /// </summary>
    public sealed class ProbabilisticAcceptanceFunction : IAcceptanceFunction
    {
        public bool Accept(double currentCost, double candidateCost, double temperature, Random rnd)
        {
            if (candidateCost <= currentCost)
                return true;

            if (temperature <= 0)
                return false;

            var probability = Math.Exp((currentCost - candidateCost) / temperature);
            return rnd.NextDouble() < probability;
        }
    }
}
