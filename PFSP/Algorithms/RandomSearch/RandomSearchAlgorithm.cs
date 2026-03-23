using PFSP.Algorithms.Monitoring;
using PFSP.Instances;
using PFSP.Solutions;
using System.Diagnostics;

namespace PFSP.Algorithms.RandomSearch
{
    // Implementation of the Random algorithm.
    public class RandomSearchAlgorithm : IAlgorithm
    {
        public AlgorithmResult Solve(Instance instance, IParameters parameters, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(instance);
            var p = parameters as RandomSearchParameters ?? throw new ArgumentException("parameters must be RandomParameters", nameof(parameters));

            var result = new AlgorithmResult();
            var stopwatch = Stopwatch.StartNew();
            var state = new RandomSearchState(instance, p, stopwatch);
            var monitor = new AlgorithmMonitor(result, p.Monitoring);

            while (state.ShouldContinue())
            {
                cancellationToken.ThrowIfCancellationRequested();
                EvaluateCandidate(state, result, monitor);
            }

            if (!result.HasBest)
                throw new InvalidOperationException("Random search finished without producing a best solution.");

            stopwatch.Stop();
            monitor.Emit(AlgorithmEventKind.Finished, state);
            return result;
        }

        private static void EvaluateCandidate(RandomSearchState state, AlgorithmResult result, AlgorithmMonitor monitor)
        {
            state.Generator.Shuffle(state.CandidatePermutation);
            var cost = state.Instance.Evaluate(state.CandidateBuffer);
            state.Candidate = PermutationSolution.WrapBuffer(state.CandidatePermutation, cost);
            state.Evaluations++;

            if (state.Best is null || cost < state.Best.Cost)
            {
                state.Best = PermutationSolution.CreateCopy(state.CandidatePermutation, cost);
                state.BestFoundAtEvaluation = state.Evaluations;
                result.SetBest(state.Best);
            }

            monitor.Emit(AlgorithmEventKind.CandidateEvaluated, state);
        }
    }
}
