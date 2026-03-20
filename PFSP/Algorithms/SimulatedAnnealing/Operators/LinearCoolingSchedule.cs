namespace PFSP.Algorithms.SimulatedAnnealing.Operators
{
    public sealed class LinearCoolingSchedule : ICoolingSchedule
    {
        public double NextTemperature(double currentTemperature, double coolingRate, int iteration, int maxIterations)
        {
            if (maxIterations <= 0)
                return 0;

            var nextTemperature = 1.0 - ((iteration - 1.0) / maxIterations);
            return nextTemperature > 0 ? nextTemperature : 0;
        }
    }
}
