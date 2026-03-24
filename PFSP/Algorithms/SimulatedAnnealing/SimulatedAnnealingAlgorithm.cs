using PFSP.Algorithms.Monitoring;
using PFSP.Algorithms.SimulatedAnnealing.Operators;
using PFSP.Instances;
using PFSP.Solutions;
using PFSP.Solutions.PermutationSolutionGenerators;

namespace PFSP.Algorithms.SimulatedAnnealing
{
    public sealed class SimulatedAnnealingAlgorithm : IAlgorithm
    {
        public SimulatedAnnealingAlgorithm()
        {
        }

        public AlgorithmResult Solve(Instance instance, IParameters parameters, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(instance);
            var parms = parameters as SimulatedAnnealingParameters ?? throw new ArgumentException("parameters must be SimulatedAnnealingParameters", nameof(parameters));

            var result = new AlgorithmResult();
            var monitor = new AlgorithmMonitor(result, parms.Monitoring);
            var state = new SimulatedAnnealingState(
                instance,
                parms,
                new RandomPermutationSolutionGenerator(parms.Seed),
                parms.Seed == 0 ? new global::System.Random() : new global::System.Random(parms.Seed))
            {
                Temperature = parms.InitialTemperature,
                Iteration = 0
            };
            state.EvaluationBudget = parms.EvaluationBudget;

            monitor.Emit(AlgorithmEventKind.Started, state);

            var initial = state.Generator.Create(instance);
            var currentPermutation = (int[])initial.Permutation.Clone();
            state.Current = PermutationSolution.CreateCopy(currentPermutation, instance.Evaluate(currentPermutation));
            state.Candidate = state.Current;
            state.Best = state.Current;
            result.SetBest(state.Current);

            monitor.Emit(AlgorithmEventKind.CandidateEvaluated, state);
            state.BestFoundAtEvaluation = state.Evaluations;
            monitor.Emit(AlgorithmEventKind.IterationCompleted, state);

            while (state.Iteration < parms.Iterations && state.Temperature > parms.MinimumTemperature)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var candidatePermutation = parms.NeighborhoodOperator.CreateNeighbor(state.Current.Permutation, state.Random);
                state.Candidate = PermutationSolution.CreateCopy(candidatePermutation, instance.Evaluate(candidatePermutation));
                state.Accepted = false;

                bool newBest = false;
                if (parms.AcceptanceFunction.Accept(state.Current.Cost, state.Candidate.Cost, state.Temperature, state.Random))
                {
                    state.Current = state.Candidate;
                    state.Accepted = true;
                    if (state.Best is null || state.Candidate.Cost < state.Best.Cost)
                    {
                        state.Best = state.Candidate;
                        result.SetBest(state.Candidate);
                        newBest = true;
                    }
                }

                monitor.Emit(AlgorithmEventKind.CandidateEvaluated, state);

                if (newBest)
                    state.BestFoundAtEvaluation = state.Evaluations;

                var coolingParameters = new CoolingScheduleParameters
                {
                    CoolingRate = parms.CoolingRate,
                    Iteration = (int)state.Iteration + 1,
                    MaxIterations = parms.Iterations,
                    MinimumTemperature = parms.MinimumTemperature,
                    OperatorParameters = parms.CoolingScheduleParameters
                };

                var nextTemperature = parms.CoolingSchedule.NextTemperature(state.Temperature, coolingParameters);
                state.Temperature = Math.Max(parms.MinimumTemperature, nextTemperature);
                state.Iteration++;
                monitor.Emit(AlgorithmEventKind.IterationCompleted, state);
            }

            monitor.Emit(AlgorithmEventKind.Finished, state);
            return result;
        }
    }
}
