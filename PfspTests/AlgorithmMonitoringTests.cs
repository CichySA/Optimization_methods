using PFSP.Algorithms;
using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.Greedy;
using PFSP.Algorithms.Monitoring;
using PFSP.Algorithms.RandomSearch;
using PFSP.Evaluators;
using PFSP.Instances;
using PFSP.Solutions;
using System.Diagnostics;

namespace PfspTests
{
    public class AlgorithmMonitoringTests
    {
        [Fact]
        public void RandomSearch_WithSelectedMetric_RecordsOnlyConfiguredSeries()
        {
            var matrix = new double[2, 5]
            {
                { 1, 2, 3, 4, 5 },
                { 5, 4, 3, 2, 1 }
            };
            var instance = Instance.Create(matrix, new TotalFlowTimeEvaluator());
            var parameters = RandomSearchParameters.ForRuns(12, seed: 7) with
            {
                Monitoring = new AlgorithmMonitoringOptions
                {
                    EnabledMetrics = [AlgorithmMetricNames.BestCostByEvaluation]
                }
            };

            var result = new RandomSearchAlgorithm().Solve(instance, parameters, TestContext.Current.CancellationToken);

            var points = Assert.IsType<AlgorithmMetricPoint[]>(result.ExperimentalData[AlgorithmMetricNames.BestCostByEvaluation]);
            Assert.NotEmpty(points);
            Assert.DoesNotContain(AlgorithmMetricNames.BestCostByIteration, result.ExperimentalData.Keys);
            Assert.DoesNotContain(AlgorithmMetricNames.BestByGeneration, result.ExperimentalData.Keys);
        }

        [Fact]
        public void Monitor_HandledEvents_DependOnlyOnEnabledMetrics()
        {
            var result = new AlgorithmResult();
            var monitor = new AlgorithmMonitor(result, new AlgorithmMonitoringOptions
            {
                EnabledMetrics = [AlgorithmMetricNames.BestByGeneration, AlgorithmMetricNames.ElapsedOnFinished]
            });

            Assert.True(monitor.Handles(AlgorithmEventKind.GenerationCompleted));
            Assert.True(monitor.Handles(AlgorithmEventKind.Finished));
            Assert.False(monitor.Handles(AlgorithmEventKind.CandidateEvaluated));
            Assert.False(monitor.Handles(AlgorithmEventKind.IterationCompleted));
            Assert.False(monitor.Handles(AlgorithmEventKind.StepCompleted));
        }

        [Fact]
        public void Monitor_AllowsAdditionalMetrics()
        {
            var result = new AlgorithmResult();
            var monitor = new AlgorithmMonitor(result, new AlgorithmMonitoringOptions(), [new ConstantMetric()]);
            var instance = Instance.CreateWithDefaultEvaluator(new double[1, 1] { { 1 } });
            var solution = PermutationSolution.WrapBuffer([0], 1.0);
            var stopwatch = Stopwatch.StartNew();
            var state = new GreedyAlgorithmState(instance, new GreedyParameters(), stopwatch)
            {
                Candidate = solution,
                Best = solution,
                Step = 1,
                Evaluations = 1,
                BestFoundAtEvaluation = 1
            };
            result.SetBest(solution);

            monitor.Emit(AlgorithmEventKind.StepCompleted, state);

            var points = Assert.IsType<AlgorithmMetricPoint[]>(result.ExperimentalData["CustomMetric"]);
            Assert.Single(points);
            Assert.Equal(1, points[0].Index);
            Assert.Equal(123.0, points[0].Value);
        }

        [Fact]
        public void EvolutionaryMetrics_RecordGenerationStatisticsAndFinishedElapsed()
        {
            var result = new AlgorithmResult();
            var monitor = new AlgorithmMonitor(result, new AlgorithmMonitoringOptions
            {
                EnabledMetrics =
                [
                    AlgorithmMetricNames.BestByGeneration,
                    AlgorithmMetricNames.MedianByGeneration,
                    AlgorithmMetricNames.DeviationByGeneration,
                    AlgorithmMetricNames.BestInPopulationByGeneration,
                    AlgorithmMetricNames.ElapsedOnFinished
                ]
            });
            var instance = Instance.CreateWithDefaultEvaluator(new double[1, 3] { { 1, 2, 3 } });
            var stopwatch = Stopwatch.StartNew();
            var best = PermutationSolution.WrapBuffer([0, 1, 2], 10.0);
            var state = new EvolutionaryAlgorithmState(
                instance,
                new EvolutionaryParameters(),
                stopwatch,
                new PFSP.Solutions.PermutationSolutionGenerators.RandomPermutationSolutionGenerator(1),
                new Random(1))
            {
                Generation = 0,
                Best = best,
                Evaluations = 9,
                BestFoundAtEvaluation = 3,
                Population =
                [
                    best,
                    PermutationSolution.WrapBuffer([0, 2, 1], 20.0),
                    PermutationSolution.WrapBuffer([1, 0, 2], 40.0)
                ]
            };
            result.SetBest(best);

            monitor.Emit(AlgorithmEventKind.GenerationCompleted, state);
            monitor.Emit(AlgorithmEventKind.Finished, state);

            Assert.Equal(10.0, Assert.IsType<double[]>(result.ExperimentalData[AlgorithmMetricNames.BestByGeneration])[0]);
            Assert.Equal(20.0, Assert.IsType<double[]>(result.ExperimentalData[AlgorithmMetricNames.MedianByGeneration])[0]);
            Assert.Equal(Math.Sqrt(155.55555555555554), Assert.IsType<double[]>(result.ExperimentalData[AlgorithmMetricNames.DeviationByGeneration])[0], 10);
            Assert.Single(Assert.IsType<AlgorithmMetricPoint[]>(result.ExperimentalData[AlgorithmMetricNames.ElapsedOnFinished]));
        }

        private sealed class ConstantMetric : IAlgorithmMetric
        {
            public string Name => "CustomMetric";
            public IReadOnlyCollection<AlgorithmEventKind> EventKinds => [AlgorithmEventKind.StepCompleted];

            public void Observe(AlgorithmEventKind eventKind, AlgorithmState state, AlgorithmMetricRecorder recorder)
            {
                if (eventKind == AlgorithmEventKind.StepCompleted && state is GreedyAlgorithmState greedyState)
                    recorder.RecordIndexed(Name, greedyState.Step, 123.0);
            }
        }
    }
}
