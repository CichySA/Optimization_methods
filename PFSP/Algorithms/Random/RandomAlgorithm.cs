using PFSP.Evaluators;
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

            // allocate one exact-size buffer and one view; both are reused every iteration
            int n = instance.Jobs;
            int[] work = new int[n];
            PermutationView view = new(work);
            IEvaluator evaluator = instance.Evaluator;
            int evals = 0;

            int iterations = p.UseTimeLimit ? int.MaxValue : p.Samples;
            bool useTimeLimit = p.UseTimeLimit;
            TimeSpan timeLimit = p.TimeLimit.GetValueOrDefault();

            // Bootstrap: evaluate one solution before the loop so bestCost is always valid.
            // This eliminates the `best == null` null-check and the `best.Cost` heap
            // dereference on every subsequent iteration.
            generator.ShuffleInto(work, n);
            double bestCost = evaluator.Evaluate(instance, view);
            evals++;
            var initPerm = new int[n];
            Array.Copy(work, initPerm, n);
            PermutationSolution? best = new PermutationSolution(initPerm, bestCost);
            iterations--;

            while (iterations-- > 0)
            {
                if (cancellationToken.IsCancellationRequested) break;
                if (useTimeLimit && sw.Elapsed >= timeLimit) break;
                // Hot-path overload: skips null/bounds validation on every call.
                generator.ShuffleInto(work, n);
                // view wraps `work` directly — no copy or allocation during evaluation
                double cost = evaluator.Evaluate(instance, view);
                evals++;
                // bestCost is a local — register/stack read, no heap dereference.
                if (cost < bestCost)
                {
                    bestCost = cost;
                    // only clone the buffer when we have a new best (rare relative to total iterations)
                    var permCopy = new int[n];
                    Array.Copy(work, permCopy, n);
                    best = new PermutationSolution(permCopy, cost);
                }
            }
            sw.Stop();
            return new AlgorithmResult(best, evals, sw.Elapsed);
        }
    }
}
