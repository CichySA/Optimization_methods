using PFSP.Algorithms.Monitoring;
using PFSP.Algorithms.SimulatedAnnealing;
using PFSP.Instances;

namespace PfspTests
{
    public class SimulatedAnnealingAlgorithmTests
    {
        [Fact]
        public void FixedSeed_OnTai20_5_0_IsDeterministic_AndReturnsValidPermutation()
        {
            var instance = InstanceReader.Read("tai_20_5_0");

            var parameters = SimulatedAnnealingParameters.Default with
            {
                Seed = 12345,
                Iterations = 1000,
                InitialTemperature = 100.0,
                CoolingRate = 0.995,
                MinimumTemperature = 0.0001,
                Monitoring = new AlgorithmMonitoringOptions { Enabled = true }
            };

            var algorithm = new SimulatedAnnealingAlgorithm();

            var result1 = algorithm.Solve(instance, parameters, TestContext.Current.CancellationToken);
            var result2 = algorithm.Solve(instance, parameters, TestContext.Current.CancellationToken);

            Assert.NotNull(result1.Best);
            Assert.NotNull(result2.Best);
            Assert.Equal(result1.Best.Cost, result2.Best.Cost);
            Assert.Equal(result1.Best.Permutation, result2.Best.Permutation);
            Assert.Equal(instance.Jobs, result1.Best.Permutation.Length);
            Assert.Equal(Enumerable.Range(0, instance.Jobs), result1.Best.Permutation.OrderBy(x => x));

            var evaluations = (long)result1.GetSingleDenseMetric(AlgorithmMetricNames.Evaluations);
            var bestFoundAt = (long)result1.GetSingleDenseMetric(AlgorithmMetricNames.BestFoundAtEvaluation);
            Assert.Equal(parameters.Iterations + 1, evaluations);
            Assert.True(bestFoundAt > 0);
            Assert.True(bestFoundAt <= evaluations);

            var bestByIteration = Assert.IsType<double[]>(result1.ExperimentalData[AlgorithmMetricNames.BestCostByIteration]);
            var currentByIteration = Assert.IsType<double[]>(result1.ExperimentalData[AlgorithmMetricNames.CurrentCostByIteration]);
            var temperatureByIteration = Assert.IsType<double[]>(result1.ExperimentalData[AlgorithmMetricNames.TemperatureByIteration]);
            Assert.Equal(evaluations, bestByIteration.Length);
            Assert.Equal(evaluations, currentByIteration.Length);
            Assert.Equal(evaluations, temperatureByIteration.Length);
        }
    }
}
