using System;
using System.Diagnostics;
using System.Threading;
using PFSP.Instances;
using PFSP.Solutions;
using PFSP.Solutions.PermutationSolutionGenerators;

namespace PFSP.Algorithms.Evolutionary
{
    public class EvolutionaryAlgorithm : IAlgorithm
    {
        private readonly IPermutationSolutionGenerator _generator;

        /// <param name="generator">
        /// Generator used to seed the initial population.
        /// When null, a <see cref="RandomPermutationSolutionGenerator"/> seeded from
        /// <see cref="EvolutionaryParameters.Seed"/> is created inside <see cref="Solve"/>.
        /// </param>
        public EvolutionaryAlgorithm(IPermutationSolutionGenerator? generator = null)
        {
            _generator = generator!; // may be null; resolved in Solve when params are available
        }

        public AlgorithmResult Solve(Instance instance, IParameters parameters, CancellationToken cancellationToken = default)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var p = parameters as EvolutionaryParameters ?? throw new ArgumentException("parameters must be EvolutionaryParameters", nameof(parameters));

            var generator = _generator ?? new RandomPermutationSolutionGenerator(p.Seed);
            var sw = Stopwatch.StartNew();

            int pop = Math.Max(1, p.PopulationSize);
            ISolution? best = null;
            int evals = 0;

            for (int g = 0; g < p.Generations && !cancellationToken.IsCancellationRequested; g++)
            {
                for (int i = 0; i < pop && !cancellationToken.IsCancellationRequested; i++)
                {
                    var sol = generator.Create(instance);
                    evals++;
                    if (best == null || sol.Cost < best.Cost) best = sol;
                }
            }

            sw.Stop();
            best ??= generator.Create(instance);
            return new AlgorithmResult(best, evals, sw.Elapsed);
        }
    }
}
