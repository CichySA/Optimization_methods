namespace PFSP.Algorithms.SimulatedAnnealing.Operators
{
    public interface IAcceptanceFunction
    {
        bool Accept(double currentCost, double candidateCost, double temperature, global::System.Random rnd);
    }
}
