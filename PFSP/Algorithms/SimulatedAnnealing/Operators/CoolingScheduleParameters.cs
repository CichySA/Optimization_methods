using PFSP.Algorithms.SimulatedAnnealing.Operators.CoolingSchedules;

namespace PFSP.Algorithms.SimulatedAnnealing.Operators
{
    public sealed record CoolingScheduleParameters
    {
        public double CoolingRate { get; init; }
        public int Iteration { get; init; }
        public int MaxIterations { get; init; }
        public double MinimumTemperature { get; init; }
        public ICoolingScheduleParameters OperatorParameters { get; init; } = new LinearCoolingScheduleParameters();
    }
}
