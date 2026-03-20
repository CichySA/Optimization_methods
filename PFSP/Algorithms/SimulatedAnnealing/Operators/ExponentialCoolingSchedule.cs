namespace PFSP.Algorithms.SimulatedAnnealing.Operators
{
    public sealed class ExponentialCoolingSchedule : ICoolingSchedule
    {
        public double NextTemperature(double currentTemperature, double coolingRate, int iteration, int maxIterations)
        {
            return currentTemperature * coolingRate;
        }
    }
}
