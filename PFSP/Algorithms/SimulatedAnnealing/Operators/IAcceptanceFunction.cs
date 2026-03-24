namespace PFSP.Algorithms.SimulatedAnnealing.Operators
{
    public interface IAcceptanceFunction
    {
        string Name { get; }
        bool Accept(double currentCost, double candidateCost, double temperature, global::System.Random rnd);
    }
}
