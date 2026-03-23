using PFSP.Algorithms.Monitoring;
using PFSP.Algorithms.SimulatedAnnealing.Operators;
using PFSP.Algorithms.SimulatedAnnealing.Operators.AcceptanceFunctions;
using PFSP.Algorithms.SimulatedAnnealing.Operators.CoolingSchedules;
using PFSP.Algorithms.SimulatedAnnealing.Operators.NeighborhoodOperators;

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
        public AlgorithmMonitoringOptions Monitoring { get; init; } = new();

        public INeighborhoodOperator NeighborhoodOperator { get; init; } = new SwapNeighborhood();
        public IAcceptanceFunction AcceptanceFunction { get; init; } = new ProbabilisticAcceptanceFunction();
        public ICoolingSchedule CoolingSchedule { get; init; } = new LinearCoolingSchedule();
        public ICoolingScheduleParameters CoolingScheduleParameters { get; init; } = new LinearCoolingScheduleParameters();

        public static SimulatedAnnealingParameters Default => new();
    }
}
