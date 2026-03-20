namespace PFSP.Algorithms.SimulatedAnnealing.Operators
{
    public interface ICoolingSchedule
    {
        double NextTemperature(double currentTemperature, double coolingRate, int iteration, int maxIterations);
    }
}
