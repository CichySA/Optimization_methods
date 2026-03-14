using PFSP.Instances;
using PFSP.Solutions;
using PFSP.Solutions.PermutationSolutionGenerators;
using System.Diagnostics;

namespace PFSP.Algorithms.Random
{
    public class RandomAlgorithm : IAlgorithm
    {
        public RandomAlgorithm()
        {
        }

        public AlgorithmResult Solve(Instance instance, IParameters parameters, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(instance);
            RandomParameters p = parameters as RandomParameters ?? throw new ArgumentException("parameters must be RandomParameters", nameof(parameters));

            RandomPermutationSolutionGenerator generator = new(p.Seed);
            var sw = Stopwatch.StartNew();

            PermutationSolution? best = null;
            int evals = 0;

            int iterations = p.UseTimeLimit ? int.MaxValue : p.Samples;

            // Allocate a single reusable candidate buffer. Because PermutationSolution
            // stores the array by reference, the evaluator always sees the current shuffle
            // without allocating a new array on every iteration.
            int n = instance.Jobs;
            int[] candidatePerm = new int[n];
            PermutationSolution candidateSol = new(candidatePerm, 0.0);

            while (iterations-- > 0)
            {
                if (cancellationToken.IsCancellationRequested) break;
                if (p.UseTimeLimit && sw.Elapsed >= p.TimeLimit) break;

                generator.Shuffle(candidatePerm);
                double cost = instance.Evaluate(candidateSol);
                evals++;

                // Only copy the permutation array when a genuine improvement is found.
                if (best == null || cost < best.Cost)
                    best = new PermutationSolution(candidatePerm.ToArray(), cost);
            }

            sw.Stop();
            if (best == null)
            {
                generator.Shuffle(candidatePerm);
                best = new PermutationSolution(candidatePerm.ToArray(), instance.Evaluate(candidateSol));
            }
            return new AlgorithmResult(best, evals, sw.Elapsed);
        }
    }
}
