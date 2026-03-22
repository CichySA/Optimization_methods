using PFSP.Algorithms.SimulatedAnnealing.Operators;

namespace PFSP.Algorithms.SimulatedAnnealing.Operators.CoolingSchedules
{
    public sealed record ExponentialCoolingScheduleParameters : ICoolingScheduleParameters
    {
        public double? DecayMultiplier { get; init; }
    }
}
