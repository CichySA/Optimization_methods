using PFSP.Algorithms;
using PFSP.Algorithms.Monitoring;
using PFSP.Algorithms.Evolutionary;
using PFSP.Instances;

namespace PfspTests
{
    public class EvolutionaryAlgorithmTests
    {
        [Fact]
        public void FixedSeed_OnTai20_5_0_IsDeterministic_AndReturnsValidPermutation()
        {
            var instance = InstanceReader.Read("tai20_5_0");

            var parameters = EvolutionaryParameters.Default;
            parameters.Seed = 12345;
            parameters.PopulationSize = 50;
            parameters.Generations = 50;
            parameters.Monitoring = new AlgorithmMonitoringOptions { Enabled = true };

            var algorithm = new EvolutionaryAlgorithm();

            var result1 = algorithm.Solve(instance, parameters, TestContext.Current.CancellationToken);
            var result2 = algorithm.Solve(instance, parameters, TestContext.Current.CancellationToken);

            Assert.NotNull(result1.Best);
            Assert.NotNull(result2.Best);
            Assert.Equal(result1.Best.Cost, result2.Best.Cost);
            Assert.Equal(result1.Best.Permutation, result2.Best.Permutation);
            Assert.Equal(instance.Jobs, result1.Best.Permutation.Length);

            var evaluations = (long)result1.GetSingleDenseMetric(AlgorithmMetricNames.Evaluations);
            var bestFoundAt = (long)result1.GetSingleDenseMetric(AlgorithmMetricNames.BestFoundAtEvaluation);

            Assert.Equal(Enumerable.Range(0, instance.Jobs), result1.Best.Permutation.OrderBy(x => x));
            Assert.Equal(
                (long)parameters.PopulationSize + (long)(parameters.PopulationSize - parameters.ElitismK) * (parameters.Generations - 1),
                evaluations);
            Assert.True(bestFoundAt > 0);
            Assert.True(bestFoundAt <= evaluations);

            var bestByGeneration = Assert.IsType<double[]>(result1.ExperimentalData[AlgorithmMetricNames.BestByGeneration]);
            var medianByGeneration = Assert.IsType<double[]>(result1.ExperimentalData[AlgorithmMetricNames.MedianByGeneration]);
            var deviationByGeneration = Assert.IsType<double[]>(result1.ExperimentalData[AlgorithmMetricNames.DeviationByGeneration]);
            var bestInPopulationByGeneration = Assert.IsType<double[]>(result1.ExperimentalData[AlgorithmMetricNames.BestInPopulationByGeneration]);
            var elapsedOnFinished = Assert.IsType<AlgorithmMetricPoint[]>(result1.ExperimentalData[AlgorithmMetricNames.ElapsedOnFinished]);
            Assert.Equal(parameters.Generations, bestByGeneration.Length);
            Assert.Equal(parameters.Generations, medianByGeneration.Length);
            Assert.Equal(parameters.Generations, deviationByGeneration.Length);
            Assert.Equal(parameters.Generations, bestInPopulationByGeneration.Length);
            Assert.Single(elapsedOnFinished);
        }
    }
}
