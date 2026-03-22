namespace PFSP.Algorithms.SimulatedAnnealing.Operators
{
    public interface ICoolingSchedule
    {
        double NextTemperature(double currentTemperature, CoolingScheduleParameters parameters);
    }
}
