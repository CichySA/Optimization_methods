using PFSP.Algorithms.SimulatedAnnealing.Operators;

namespace PFSP.Algorithms.SimulatedAnnealing.Operators.CoolingSchedules
{
    public sealed class LinearCoolingSchedule : ICoolingSchedule
    {
        public const string Name = "Linear";
        string ICoolingSchedule.Name => Name;

        public double NextTemperature(double currentTemperature, CoolingScheduleParameters parameters)
        {
            if (parameters.OperatorParameters is not LinearCoolingScheduleParameters)
                throw new ArgumentException($"{nameof(LinearCoolingSchedule)} requires {nameof(LinearCoolingScheduleParameters)}.");

            if (parameters.MaxIterations <= 0)
                return 0;

            var nextTemperature = 1.0 - ((parameters.Iteration - 1.0) / parameters.MaxIterations);
            return nextTemperature > 0 ? nextTemperature : 0;
        }
    }
}
