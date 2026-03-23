using ExperimentRunner;
using PFSP.Algorithms;
using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.Monitoring;
using PFSP.Solutions;

namespace PfspTests.ExperimentRunner
{
    [Trait("Area", "ExperimentRunner")]
    [Trait("Component", "ResultSerialization")]
    [Trait("Kind", "Unit")]
    public class ExperimentRunRecordFactoryTests
    {
        [Fact]
        public void Create_CompressesDenseZeroBasedMetricsToSimpleArrays()
        {
            var parameters = new EvolutionaryParameters { Seed = 42, PopulationSize = 10, Generations = 5 };
            var result = new AlgorithmResult();
            var recorder = new AlgorithmMetricRecorder(result);
            result.SetBest(PermutationSolution.WrapBuffer([0, 1, 2], 12.5));
            recorder.RecordDense(AlgorithmMetricNames.Evaluations, 30);
            recorder.RecordDense(AlgorithmMetricNames.ElapsedMs, 123);
            recorder.RecordDense(AlgorithmMetricNames.BestFoundAtEvaluation, 7);
            recorder.RecordDense("BestByGeneration", 20.0);
            recorder.RecordDense("BestByGeneration", 12.5);
            recorder.RecordDense("MedianByGeneration", 25.0);
            recorder.RecordDense("MedianByGeneration", 15.0);
            recorder.RecordIndexed("ElapsedOnFinished", 30, 123.0);

            var record = ExperimentRunRecordFactory.Create(
                "tai_20_5_0",
                "Evolutionary_p10_g5_s42",
                parameters,
                parameters.Seed,
                result,
                DateTimeOffset.UnixEpoch);

            Assert.Equal("tai_20_5_0", record.Instance);
            Assert.Equal("Evolutionary_p10_g5_s42", record.Algorithm);
            Assert.Equal(42, record.Seed);
            Assert.Equal(12.5, record.Best.Cost);
            Assert.Equal([0, 1, 2], record.Best.Permutation);
            Assert.Equal([30.0], Assert.IsType<double[]>(record.Metrics[AlgorithmMetricNames.Evaluations]));
            Assert.Equal([123.0], Assert.IsType<double[]>(record.Metrics[AlgorithmMetricNames.ElapsedMs]));
            Assert.Equal([7.0], Assert.IsType<double[]>(record.Metrics[AlgorithmMetricNames.BestFoundAtEvaluation]));
            Assert.Equal([20.0, 12.5], Assert.IsType<double[]>(record.Metrics["BestByGeneration"]));
            Assert.Equal([25.0, 15.0], Assert.IsType<double[]>(record.Metrics["MedianByGeneration"]));

            var elapsedOnFinished = Assert.IsType<AlgorithmMetricPoint[]>(record.Metrics["ElapsedOnFinished"]);
            Assert.Single(elapsedOnFinished);
            Assert.Equal(30, elapsedOnFinished[0].Index);
            Assert.Equal(123.0, elapsedOnFinished[0].Value);
        }
    }
}
