namespace PFSP.Algorithms.SimulatedAnnealing.Operators
{
    public interface ICoolingSchedule
    {
        string Name { get; }
        double NextTemperature(double currentTemperature, CoolingScheduleParameters parameters);
    }
}
