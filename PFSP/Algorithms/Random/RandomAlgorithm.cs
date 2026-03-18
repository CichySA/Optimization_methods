using PFSP.Instances;
using PFSP.Solutions;
using PFSP.Solutions.PermutationSolutionGenerators;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PFSP.Algorithms.Random
{
    // Sequential implementation of the Random algorithm.
    public class RandomAlgorithm : IAlgorithm
    {
        public RandomAlgorithm()
        {
        }

        public AlgorithmResult Solve(Instance instance, IParameters parameters, CancellationToken cancellationToken = default)
        {
            var p = RandomAlgorithmCore.ParseParameters(instance, parameters);
            var sw = Stopwatch.StartNew();
            bool useTimeLimit = p.TimeLimit.HasValue;
            TimeSpan deadline = useTimeLimit ? p.TimeLimit!.Value : TimeSpan.Zero;

            var (best, evals) = RandomAlgorithmCore.RunWorker(
                instance, p.Seed, p.Samples, sw, deadline, useTimeLimit, cancellationToken);

            sw.Stop();
            return new AlgorithmResult(RandomAlgorithmCore.EnsureBest(best, instance, p.Seed), evals, sw.Elapsed)
            {
                BestFoundAtEvaluation = -1
            };
        }
    }

    // Parallel implementation of the Random algorithm.
    public class ParallelRandomAlgorithm : IAlgorithm
    {
        public ParallelRandomAlgorithm()
        {
        }

        public AlgorithmResult Solve(Instance instance, IParameters parameters, CancellationToken cancellationToken = default)
        {
            var p = RandomAlgorithmCore.ParseParameters(instance, parameters);
            int workers = Environment.ProcessorCount;  
            var sw = Stopwatch.StartNew();
            bool useTimeLimit = p.TimeLimit.HasValue;
            TimeSpan deadline = useTimeLimit ? p.TimeLimit!.Value : TimeSpan.Zero;

            // divide work evenly among workers
            long totalSamples = useTimeLimit ? 0 : p.Samples;
            long baseIters = useTimeLimit ? 0 : totalSamples / workers;
            long remainder = useTimeLimit ? 0 : totalSamples % workers;

            PermutationSolution?[] bestPerWorker = new PermutationSolution?[workers];
            long[] evalsPerWorker = new long[workers];

            var po = new ParallelOptions
            {
                MaxDegreeOfParallelism = workers,
                CancellationToken = cancellationToken
            };

            try
            {
                Parallel.For(0, workers, po, workerId =>
                {
                    int seed = p.Seed == 0 ? 0 : unchecked(p.Seed + workerId * 100_0003);
                    long iterations = useTimeLimit ? 0 : baseIters + (workerId < remainder ? 1 : 0);

                    var (best, evals) = RandomAlgorithmCore.RunWorker(
                        instance, seed, iterations, sw, deadline, useTimeLimit, po.CancellationToken);

                    bestPerWorker[workerId] = best;
                    evalsPerWorker[workerId] = evals;
                });
            }
            catch (OperationCanceledException)
            {
                // expected when cancellationToken is triggered
            }

            sw.Stop();

            long totalEvals = 0;
            for (int i = 0; i < workers; i++) totalEvals += evalsPerWorker[i];

            PermutationSolution? globalBest = null;
            for (int i = 0; i < workers; i++)
            {
                var b = bestPerWorker[i];
                if (b == null) continue;
                if (globalBest == null || b.Cost < globalBest.Cost) globalBest = b;
            }

            return new AlgorithmResult(RandomAlgorithmCore.EnsureBest(globalBest, instance, p.Seed), totalEvals, sw.Elapsed)
            {
                // In parallel this has ambiguous meaning
                BestFoundAtEvaluation = -1
            };
        }
    }

    // Shared core logic
    internal static class RandomAlgorithmCore
    {
        internal static RandomParameters ParseParameters(Instance instance, IParameters parameters)
        {
            ArgumentNullException.ThrowIfNull(instance);
            return parameters as RandomParameters
                ?? throw new ArgumentException("parameters must be RandomParameters", nameof(parameters));
        }

        internal static (PermutationSolution? best, long evals) RunWorker(
            Instance instance,
            int seed,
            long iterations,
            Stopwatch sw,
            TimeSpan deadline,
            bool useTimeLimit,
            CancellationToken cancellationToken)
        {
            var generator = new RandomPermutationSolutionGenerator(seed);
            int n = instance.Jobs;
            int[] candidatePerm = new int[n];
            PermutationSolution candidateSol = PermutationSolution.WrapBuffer(candidatePerm, 0.0);

            PermutationSolution? best = null;
            long evals = 0;

            if (!useTimeLimit)
            {
                while (iterations-- > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    generator.Shuffle(candidatePerm);
                    double cost = instance.Evaluate(candidateSol);
                    evals++;
                    if (best == null || cost < best.Cost)
                        best = PermutationSolution.CreateCopy(candidatePerm, cost);
                }
            }
            else
            {
                while (sw.Elapsed < deadline)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    generator.Shuffle(candidatePerm);
                    double cost = instance.Evaluate(candidateSol);
                    evals++;
                    if (best == null || cost < best.Cost)
                        best = PermutationSolution.CreateCopy(candidatePerm, cost);
                }
            }

            return (best, evals);
        }

        // If no solution was found (e.g. due to immediate cancellation), generate a fallback random solution.
        internal static PermutationSolution EnsureBest(PermutationSolution? best, Instance instance, int seed)
        {
            if (best != null) return best;
            var generator = new RandomPermutationSolutionGenerator(seed);
            int n = instance.Jobs;
            int[] fallbackPerm = new int[n];
            var fallbackSol = PermutationSolution.WrapBuffer(fallbackPerm, 0.0);
            generator.Shuffle(fallbackPerm);
            return PermutationSolution.CreateCopy(fallbackPerm, instance.Evaluate(fallbackSol));
        }
    }
}
