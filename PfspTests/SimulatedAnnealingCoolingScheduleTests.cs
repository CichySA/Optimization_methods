using PFSP.Algorithms.SimulatedAnnealing;
using PFSP.Algorithms.SimulatedAnnealing.Operators;

namespace PfspTests
{
    public class SimulatedAnnealingCoolingScheduleTests
    {
        [Fact]
        public void LinearCoolingSchedule_DecreasesLinearly_AndStopsAtZero()
        {
            var schedule = new LinearCoolingSchedule();

            var t1 = schedule.NextTemperature(100.0, 5.0, 1, 10);
            var t2 = schedule.NextTemperature(100.0, 5.0, 2, 10);
            var t3 = schedule.NextTemperature(100.0, 5.0, 3, 10);
            var t10 = schedule.NextTemperature(100.0, 5.0, 10, 10);

            Assert.Equal(1.0, t1, 10);
            Assert.Equal(0.9, t2, 10);
            Assert.Equal(0.8, t3, 10);
            Assert.Equal(0.1, t10, 10);
        }

        [Fact]
        public void ParameterFactory_RegistersLinearCoolingSchedule()
        {
            var names = SimulatedAnnealingParameterFactory.CoolingRegistry.Keys;
            Assert.Contains(SimulatedAnnealingParameterFactory.LinearCoolingName, names);
        }
    }
}
