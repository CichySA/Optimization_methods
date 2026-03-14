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

            var generator = new RandomPermutationSolutionGenerator(p.Seed);
            var sw = Stopwatch.StartNew();

            PermutationSolution? best = null;
            long evals = 0;
            long bestFoundAtEval = -1;

            // Number of iterations when using samples; when using TimeLimit we loop until time expires.
            long iterations = p.TimeLimit.HasValue ? long.MaxValue : p.Samples;

            int n = instance.Jobs;
            int[] candidatePerm = new int[n];
            // Wrap buffer to avoid copying on every evaluation; explicit about ownership.
            PermutationSolution candidateSol = PermutationSolution.WrapBuffer(candidatePerm, 0.0);

            while (iterations-- > 0)
            {
                if (cancellationToken.IsCancellationRequested) break;
                if (p.TimeLimit.HasValue && sw.Elapsed >= p.TimeLimit.Value) break;

                generator.Shuffle(candidatePerm);
                double cost = instance.Evaluate(candidateSol);
                evals++;

                // Only copy the permutation array when a genuine improvement is found.
                if (best == null || cost < best.Cost)
                {
                    best = PermutationSolution.CreateCopy(candidatePerm, cost);
                    bestFoundAtEval = evals;
                }
            }

            sw.Stop();
            if (best == null)
            {
                generator.Shuffle(candidatePerm);
                best = PermutationSolution.CreateCopy(candidatePerm, instance.Evaluate(candidateSol));
                bestFoundAtEval = evals;
            }

            return new AlgorithmResult(best, evals, sw.Elapsed) { BestFoundAtEvaluation = bestFoundAtEval };
        }
    }
}