namespace PFSP.Algorithms.SimulatedAnnealing.Operators
{
    /// <summary>
    ///     based on Kirkpatrick et al. (1983)
    /// </summary>
    public sealed class ProbabilisticAcceptanceAlgorithm : IAcceptanceFunction
    {
        public bool Accept(double currentCost, double candidateCost, double temperature, Random rnd)
        {
            if (candidateCost <= currentCost)
                return true;

            if (temperature <= 0)
                return false;

            // exp(-(e'-e)/T)
            var probability = Math.Exp((currentCost - candidateCost) / temperature);
            return rnd.NextDouble() < probability;
        }
    }
}
