using PFSP.Algorithms.SimulatedAnnealing.Operators;

namespace PFSP.Algorithms.SimulatedAnnealing.Operators.CoolingSchedules
{
    public sealed class ExponentialCoolingSchedule : ICoolingSchedule
    {
        public double NextTemperature(double currentTemperature, CoolingScheduleParameters parameters)
        {
            if (parameters.OperatorParameters is not ExponentialCoolingScheduleParameters exponential)
                throw new ArgumentException($"{nameof(ExponentialCoolingSchedule)} requires {nameof(ExponentialCoolingScheduleParameters)}.");

            var multiplier = exponential.DecayMultiplier ?? parameters.CoolingRate;
            return currentTemperature * multiplier;
        }
    }
}
