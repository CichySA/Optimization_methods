using PFSP.Instances;
using PFSP.Solutions;
using PFSP.Solutions.PermutationSolutionGenerators;
using System.Diagnostics;

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
            var rnd = parms.Seed == 0 ? new Random() : new Random(parms.Seed);

            var sw = Stopwatch.StartNew();
            long evaluations = 0;
            long bestFoundAt = -1;

            var gen = new RandomPermutationSolutionGenerator(parms.Seed);
            var initial = gen.Create(instance);

            var currentPerm = (int[])initial.Permutation.Clone();
            var currentCost = instance.Evaluate(currentPerm);
            evaluations++;

            var current = PermutationSolution.CreateCopy(currentPerm, currentCost);
            var best = current;
            bestFoundAt = evaluations;

            double temperature = parms.InitialTemperature;

            for (int iteration = 0; iteration < parms.Iterations && temperature > parms.MinimumTemperature; iteration++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var candidatePerm = parms.NeighborhoodOperator.CreateNeighbor(current.Permutation, rnd);
                var candidateCost = instance.Evaluate(candidatePerm);
                evaluations++;

                if (parms.AcceptanceFunction.Accept(current.Cost, candidateCost, temperature, rnd))
                {
                    current = PermutationSolution.CreateCopy(candidatePerm, candidateCost);
                    if (candidateCost < best.Cost)
                    {
                        best = current;
                        bestFoundAt = evaluations;
                    }
                }

                temperature = parms.CoolingSchedule.NextTemperature(temperature, parms.CoolingRate, iteration + 1, parms.Iterations);
            }

            sw.Stop();

            return new AlgorithmResult(best, evaluations, sw.Elapsed)
            {
                BestFoundAtEvaluation = bestFoundAt
            };
        }
    }
}
