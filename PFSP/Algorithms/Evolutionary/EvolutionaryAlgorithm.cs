using System;
using System.Diagnostics;
using System.Threading;
using PFSP.Instances;
using PFSP.Solutions;

using PFSP.Solutions.PermutationSolutionGenerators;
using PFSP.Algorithms.Evolutionary;

namespace PFSP.Algorithms.Evolutionary
{
    public class EvolutionaryAlgorithm : IAlgorithm
    {
        public EvolutionaryAlgorithm()
        {
        }

        public AlgorithmResult Solve(Instance instance, IParameters parameters, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(instance);
            var p = parameters as EvolutionaryParameters ?? throw new ArgumentException("parameters must be EvolutionaryParameters", nameof(parameters));
            var gen = new RandomPermutationSolutionGenerator(p.Seed);
            var rnd = p.Seed == 0 ? new System.Random() : new System.Random(p.Seed);

            var sw = Stopwatch.StartNew();
            long evaluations = 0;
            PermutationSolution best = null!;
            long bestFoundAt = -1;

            // Initial population
            var populationSize = Math.Max(1, p.PopulationSize);
            var population = new PermutationSolution[populationSize];
            for (int i = 0; i < populationSize; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var sol = gen.Create(instance);
                var cost = instance.Evaluate(sol);
                evaluations++;
                sol = sol with { Cost = cost };
                population[i] = sol;

                if (best == null || sol.Cost < best.Cost)
                {
                    best = sol;
                    bestFoundAt = evaluations;
                }
            }

            // Evolution loop
            for (int genIndex = 0; genIndex < p.Generations; genIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var newPop = new PermutationSolution[populationSize];
                int filled = 0;

                while (filled < populationSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();



                    // Selection
                    var parent1 = p.SelectionMethod.Select(population, rnd, p.TournamentSize);
                    var parent2 = p.SelectionMethod.Select(population, rnd, p.TournamentSize);

                    int[] childPerm1, childPerm2;

                    // Crossover
                    if (rnd.NextDouble() < p.CrossoverRate)
                    {
                        childPerm1 = p.CrossoverMethod.Crossover(parent1.Permutation, parent2.Permutation, rnd);
                        childPerm2 = p.CrossoverMethod.Crossover(parent2.Permutation, parent1.Permutation, rnd);
                    }
                    else
                    {
                        // Copy parents
                        childPerm1 = (int[])parent1.Permutation.Clone();
                        childPerm2 = (int[])parent2.Permutation.Clone();
                    }

                    // Mutation
                    if (rnd.NextDouble() < p.MutationRate)
                        p.MutationMethod.Mutate(childPerm1, rnd);
                    if (rnd.NextDouble() < p.MutationRate)
                        p.MutationMethod.Mutate(childPerm2, rnd);

                    // Evaluate and add to new population
                    var child1 = PermutationSolution.CreateCopy(childPerm1, instance.Evaluate(childPerm1));
                    evaluations++;
                    newPop[filled++] = child1;
                    if (child1.Cost < best.Cost)
                    {
                        best = child1;
                        bestFoundAt = evaluations;
                    }

                    if (filled < populationSize)
                    {
                        var child2 = PermutationSolution.CreateCopy(childPerm2, instance.Evaluate(childPerm2));
                        evaluations++;
                        newPop[filled++] = child2;
                        if (child2.Cost < best.Cost)
                        {
                            best = child2;
                            bestFoundAt = evaluations;
                        }
                    }
                }

                population = newPop;
            }

            sw.Stop();

            var result = new AlgorithmResult(best, evaluations, sw.Elapsed)
            {
                BestFoundAtEvaluation = bestFoundAt
            };

            return result;
        }

    }
}
