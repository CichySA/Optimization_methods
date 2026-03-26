using PFSP.Algorithms;
using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.SimulatedAnnealing;
using PFSP.Solutions;

namespace PFSP.Monitoring
{
    public static class StandardAlgorithmMetrics
    {
        public static IReadOnlyList<IAlgorithmMetric> Create() =>
        [
            new DenseMetric<AlgorithmState>(
                AlgorithmMetricNames.Evaluations,
                AlgorithmEventKind.Finished,
                state => state.Evaluations > 0 ? state.Evaluations : null),

            new DenseMetric<AlgorithmState>(
                AlgorithmMetricNames.ElapsedMs,
                AlgorithmEventKind.Finished,
                state => state.Elapsed.TotalMilliseconds),

            new DenseMetric<AlgorithmState>(
                AlgorithmMetricNames.BestFoundAtEvaluation,
                AlgorithmEventKind.Finished,
                state => state.BestFoundAtEvaluation >= 0 ? state.BestFoundAtEvaluation : null),

            new IndexedMetric<AlgorithmState>(
                AlgorithmMetricNames.BestCostByEvaluation,
                AlgorithmEventKind.CandidateEvaluated,
                state => state.Best is null ? null : (state.Evaluations, state.Best.Cost)),

            new DenseMetric<SimulatedAnnealingState>(
                AlgorithmMetricNames.BestCostByIteration,
                AlgorithmEventKind.IterationCompleted,
                state => state.Best is null ? null : state.Best.Cost),

            new DenseMetric<SimulatedAnnealingState>(
                AlgorithmMetricNames.CurrentCostByIteration,
                AlgorithmEventKind.IterationCompleted,
                state => state.Current.Cost),

            new DenseMetric<SimulatedAnnealingState>(
                AlgorithmMetricNames.TemperatureByIteration,
                AlgorithmEventKind.IterationCompleted,
                state => state.Temperature),

            new DenseMetric<EvolutionaryAlgorithmState>(
                AlgorithmMetricNames.BestByGeneration,
                AlgorithmEventKind.GenerationCompleted,
                state => state.Best is null ? null : state.Best.Cost),

            new DenseMetric<EvolutionaryAlgorithmState>(
                AlgorithmMetricNames.MeanByGeneration,
                AlgorithmEventKind.GenerationCompleted,
                state => MeanCost(state.Population)),

            new DenseMetric<EvolutionaryAlgorithmState>(
                AlgorithmMetricNames.DeviationByGeneration,
                AlgorithmEventKind.GenerationCompleted,
                state => StandardDeviation(state.Population)),

            new DenseMetric<EvolutionaryAlgorithmState>(
                AlgorithmMetricNames.BestInPopulationByGeneration,
                AlgorithmEventKind.GenerationCompleted,
                state => BestPopulationCost(state.Population)),

            new DenseMetric<EvolutionaryAlgorithmState>(
                AlgorithmMetricNames.WorstInPopulationByGeneration,
                AlgorithmEventKind.GenerationCompleted,
                state => WorstPopulationCost(state.Population)),

             new DenseMetric<EvolutionaryAlgorithmState>(
                AlgorithmMetricNames.HemmingDistanceByGeneration,
                AlgorithmEventKind.GenerationCompleted,
                state => HemmingDistance(state.Population))

        ];

        private sealed class DenseMetric<TState>(
            string name,
            AlgorithmEventKind eventKind,
            Func<TState, double?> selector) : IAlgorithmMetric
            where TState : AlgorithmState
        {
            public string Name => name;

            public void Observe(AlgorithmEventKind observedEventKind, AlgorithmState state, AlgorithmMetricRecorder recorder)
            {
                if (observedEventKind != eventKind || state is not TState typedState)
                    return;

                var value = selector(typedState);
                if (!value.HasValue)
                    return;

                recorder.RecordDense(name, value.Value);
            }
        }

        private sealed class IndexedMetric<TState>(
            string name,
            AlgorithmEventKind eventKind,
            Func<TState, (long Index, double Value)?> selector) : IAlgorithmMetric
            where TState : AlgorithmState
        {
            public string Name => name;

            public void Observe(AlgorithmEventKind observedEventKind, AlgorithmState state, AlgorithmMetricRecorder recorder)
            {
                if (observedEventKind != eventKind || state is not TState typedState)
                    return;

                var point = selector(typedState);
                if (!point.HasValue)
                    return;

                recorder.RecordIndexed(name, point.Value.Index, point.Value.Value);
            }
        }

        private static double MeanCost(IEnumerable<ISolution> population)
        {
            var costs = population.Select(solution => solution.Cost).ToArray();
            return costs.Length == 0 ? 0.0 : costs.Average();
        }

        private static double StandardDeviation(IEnumerable<ISolution> population)
        {
            var costs = population.Select(solution => solution.Cost).ToArray();
            if (costs.Length == 0)
                return 0.0;

            var average = costs.Average();
            var variance = costs.Select(cost => (cost - average) * (cost - average)).Average();
            return Math.Sqrt(variance);
        }

        private static double BestPopulationCost(IEnumerable<ISolution> population)
        {
            double best = double.PositiveInfinity;
            foreach (var solution in population)
                if (solution.Cost < best)
                    best = solution.Cost;

            return double.IsPositiveInfinity(best) ? 0.0 : best;
        }

        private static double WorstPopulationCost(IEnumerable<ISolution> population)
        {
            double worst = double.NegativeInfinity;
            foreach (var solution in population)
                if (solution.Cost > worst)
                    worst = solution.Cost;

            return double.IsNegativeInfinity(worst) ? 0.0 : worst;
        }

        private static double HemmingDistance(IEnumerable<ISolution> population)
        {
            var solutions = population.ToArray();
            int n = solutions.Length;
            if (n < 2)
                return 0.0;

            int permutationLength = solutions[0].Permutation.Length;
            double totalDistance = 0;
            int pairCount = 0;

            for (int i = 0; i < n - 1; i++)
            {
                var permA = solutions[i].Permutation;
                for (int j = i + 1; j < n; j++)
                {
                    var permB = solutions[j].Permutation;
                    int dist = 0;
                    for (int k = 0; k < permutationLength; k++)
                    {
                        if (permA[k] != permB[k])
                            dist++;
                    }
                    totalDistance += dist;
                    pairCount++;
                }
            }

            return pairCount > 0 ? totalDistance / pairCount : 0.0;
        }

    }
}
