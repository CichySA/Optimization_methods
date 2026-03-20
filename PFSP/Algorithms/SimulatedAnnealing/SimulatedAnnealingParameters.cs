using PFSP.Algorithms.SimulatedAnnealing.Operators;

namespace PFSP.Algorithms.SimulatedAnnealing
{
    public sealed record SimulatedAnnealingParameters : IParameters
    {
        public const int DefaultSeed = 0;
        public const int DefaultIterations = 10000;
        public const double DefaultInitialTemperature = 100.0;
        public const double DefaultCoolingRate = 0.995;
        public const double DefaultMinimumTemperature = 0.0001;

        public int Seed { get; init; } = DefaultSeed;
        public int Iterations { get; init; } = DefaultIterations;
        public double InitialTemperature { get; init; } = DefaultInitialTemperature;
        public double CoolingRate { get; init; } = DefaultCoolingRate;
        public double MinimumTemperature { get; init; } = DefaultMinimumTemperature;

        public INeighborhoodOperator NeighborhoodOperator { get; init; } = new SwapNeighborhood();
        public IAcceptanceFunction AcceptanceFunction { get; init; } = new ProbabilisticAcceptanceAlgorithm();
        public ICoolingSchedule CoolingSchedule { get; init; } = new LinearCoolingSchedule();

        public static SimulatedAnnealingParameters Default => new();
    }
}
