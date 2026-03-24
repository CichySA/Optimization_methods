using PFSP.Algorithms.SimulatedAnnealing;
using PFSP.Algorithms.SimulatedAnnealing.Operators;
using PFSP.Algorithms.SimulatedAnnealing.Operators.CoolingSchedules;

namespace PfspTests.Algorithms.SimulatedAnnealing
{
    public class SimulatedAnnealingCoolingScheduleTests
    {
        [Fact]
        public void LinearCoolingSchedule_DecreasesLinearly_AndStopsAtZero()
        {
            var schedule = new LinearCoolingSchedule();

            var t1 = schedule.NextTemperature(100.0, new CoolingScheduleParameters { Iteration = 1, MaxIterations = 10 });
            var t2 = schedule.NextTemperature(100.0, new CoolingScheduleParameters { Iteration = 2, MaxIterations = 10 });
            var t3 = schedule.NextTemperature(100.0, new CoolingScheduleParameters { Iteration = 3, MaxIterations = 10 });
            var t10 = schedule.NextTemperature(100.0, new CoolingScheduleParameters { Iteration = 10, MaxIterations = 10 });

            Assert.Equal(1.0, t1, 10);
            Assert.Equal(0.9, t2, 10);
            Assert.Equal(0.8, t3, 10);
            Assert.Equal(0.1, t10, 10);
        }

        [Fact]
        public void ParameterFactory_RegistersLinearCoolingSchedule()
        {
            var schedule = new LinearCoolingSchedule();
            Assert.Equal("Linear", LinearCoolingSchedule.Name);
        }
    }
}
