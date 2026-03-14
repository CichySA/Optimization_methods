using System;
using System.Diagnostics;
using System.Threading;
using PFSP.Instances;
using PFSP.Solutions;
using PFSP.Solutions.PermutationSolutionGenerators;

namespace PFSP.Algorithms.Greedy
{
    public class GreedyAlgorithm : IAlgorithm
    {
        private readonly IPermutationSolutionGenerator _generator;

        /// <param name="generator">
        /// Generator used to construct the initial solution.
        /// Defaults to <see cref="IdentityPermutationSolutionGenerator"/> (placeholder).
        /// </param>
        public GreedyAlgorithm(IPermutationSolutionGenerator? generator = null)
        {
            _generator = generator ?? new IdentityPermutationSolutionGenerator();
        }

        public AlgorithmResult Solve(Instance instance, IParameters parameters, CancellationToken cancellationToken = default)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var p = parameters as GreedyParameters ?? throw new ArgumentException("parameters must be GreedyParameters", nameof(parameters));

            var sw = Stopwatch.StartNew();
            var sol = _generator.Create(instance);
            sw.Stop();

            return new AlgorithmResult(sol, Evaluations: 1, Elapsed: sw.Elapsed);
        }
    }
}
