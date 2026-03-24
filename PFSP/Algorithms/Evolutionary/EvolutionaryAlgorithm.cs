using PFSP.Algorithms.Monitoring;
using PFSP.Instances;
using PFSP.Solutions;
using PFSP.Solutions.PermutationSolutionGenerators;

namespace PFSP.Algorithms.Evolutionary
{
    public class EvolutionaryAlgorithm : IAlgorithm
    {
        public AlgorithmResult Solve(Instance instance, IParameters parameters, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(instance);
            var parms = parameters as EvolutionaryParameters ?? throw new ArgumentException("parameters must be EvolutionaryParameters", nameof(parameters));

            var result = new AlgorithmResult();
            var monitor = new AlgorithmMonitor(result, parms.Monitoring);
            var state = new EvolutionaryAlgorithmState(
                instance,
                parms,
                new RandomPermutationSolutionGenerator(parms.Seed),
                parms.Seed == 0 ? new Random() : new Random(parms.Seed))
            {
                // EvaluationBudget = 0 means no constraint; positive value is the user-provided NFE limit.
                EvaluationBudget = parms.EvaluationBudget
            };

            monitor.Emit(AlgorithmEventKind.Started, state);

            for (int i = 0; i < state.PopulationSize; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var solution = state.Generator.Create(instance);
                solution = solution with { Cost = instance.Evaluate(solution) };
                state.Candidate = solution;
                state.Population[i] = solution;

                bool newBest = state.Best is null || solution.Cost < state.Best.Cost;
                if (newBest)
                {
                    state.Best = solution;
                    result.SetBest(solution);
                }

                monitor.Emit(AlgorithmEventKind.CandidateEvaluated, state);

                if (newBest)
                    state.BestFoundAtEvaluation = state.Evaluations;
            }

            state.Generation = 0;
            monitor.Emit(AlgorithmEventKind.GenerationCompleted, state);

            for (int generation = 0; generation < parms.Generations; generation++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var nextPopulation = new PermutationSolution[state.PopulationSize];
                int filled = 0;

                // Elitism: carry over the top k individuals without re-evaluation
                if (parms.ElitismK > 0)
                {
                    foreach (var elite in state.Population.OrderBy(s => s.Cost).Take(parms.ElitismK))
                        nextPopulation[filled++] = elite;
                }

                while (filled < state.PopulationSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var parent1 = parms.SelectionMethod.Select(state.Population, state.Random, parms.SelectionParameters);
                    var parent2 = parms.SelectionMethod.Select(state.Population, state.Random, parms.SelectionParameters);

                    int[] childPermutation1;
                    int[] childPermutation2;

                    if (state.Random.NextDouble() < parms.CrossoverRate)
                    {
                        childPermutation1 = parms.CrossoverMethod.Crossover(parent1.Permutation, parent2.Permutation, state.Random);
                        childPermutation2 = parms.CrossoverMethod.Crossover(parent2.Permutation, parent1.Permutation, state.Random);
                    }
                    else
                    {
                        childPermutation1 = (int[])parent1.Permutation.Clone();
                        childPermutation2 = (int[])parent2.Permutation.Clone();
                    }

                    if (state.Random.NextDouble() < parms.MutationRate)
                        parms.MutationMethod.Mutate(childPermutation1, state.Random);
                    if (state.Random.NextDouble() < parms.MutationRate)
                        parms.MutationMethod.Mutate(childPermutation2, state.Random);

                    var child1 = PermutationSolution.CreateCopy(childPermutation1, instance.Evaluate(childPermutation1));
                    state.Candidate = child1;
                    nextPopulation[filled++] = child1;
                    bool newBest1 = state.Best is null || child1.Cost < state.Best.Cost;
                    if (newBest1)
                    {
                        state.Best = child1;
                        result.SetBest(child1);
                    }
                    monitor.Emit(AlgorithmEventKind.CandidateEvaluated, state);
                    if (newBest1)
                        state.BestFoundAtEvaluation = state.Evaluations;

                    if (filled >= state.PopulationSize)
                        break;

                    var child2 = PermutationSolution.CreateCopy(childPermutation2, instance.Evaluate(childPermutation2));
                    state.Candidate = child2;
                    nextPopulation[filled++] = child2;
                    bool newBest2 = state.Best is null || child2.Cost < state.Best.Cost;
                    if (newBest2)
                    {
                        state.Best = child2;
                        result.SetBest(child2);
                    }
                    monitor.Emit(AlgorithmEventKind.CandidateEvaluated, state);
                    if (newBest2)
                        state.BestFoundAtEvaluation = state.Evaluations;
                }

                state.Population = nextPopulation;
                state.Generation = generation + 1;
                monitor.Emit(AlgorithmEventKind.GenerationCompleted, state);
            }

            monitor.Emit(AlgorithmEventKind.Finished, state);
            result.EvaluationBudget = state.EvaluationBudget;
            return result;
        }
    }
}
