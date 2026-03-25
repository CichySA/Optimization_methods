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

            var result = new AlgorithmResult(parameters);
            var state = new RandomSearchState(instance, p);
            var monitor = new AlgorithmMonitor(result, p.Monitoring);

            monitor.Emit(AlgorithmEventKind.Started, state);

            while (state.ShouldContinue())
            {
                cancellationToken.ThrowIfCancellationRequested();
                EvaluateCandidate(state, result, monitor);
            }

            if (!result.HasBest)
                throw new InvalidOperationException("Random search finished without producing a best solution.");

            monitor.Emit(AlgorithmEventKind.Finished, state);
            return result;
        }

        private static void EvaluateCandidate(RandomSearchState state, AlgorithmResult result, AlgorithmMonitor monitor)
        {
            state.Generator.Shuffle(state.CandidatePermutation);
            var cost = state.Instance.Evaluate(state.CandidateBuffer);
            state.Candidate = PermutationSolution.WrapBuffer(state.CandidatePermutation, cost);

            bool improved = state.Best is null || cost < state.Best.Cost;
            if (improved)
            {
                state.Best = PermutationSolution.CreateCopy(state.CandidatePermutation, cost);
                result.SetBest(state.Best);
            }

            monitor.Emit(AlgorithmEventKind.CandidateEvaluated, state);

            if (improved)
                state.BestFoundAtEvaluation = state.Evaluations;
        }
    }
}
