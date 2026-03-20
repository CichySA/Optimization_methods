namespace PFSP.Algorithms.SimulatedAnnealing.Operators
{
    /// <summary>
    /// based on Dueck and Scheuer (1990)
    /// </summary>
    public sealed class ThresholdAcceptanceFunction : IAcceptanceFunction
    {
        public bool Accept(double currentCost, double candidateCost, double temperature, Random rnd)
        {
            return candidateCost <= currentCost + temperature;
        }
    }
}
