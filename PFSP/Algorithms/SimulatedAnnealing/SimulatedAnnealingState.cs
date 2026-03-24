using PFSP.Algorithms.Monitoring;
using PFSP.Instances;
using PFSP.Solutions;
using PFSP.Solutions.PermutationSolutionGenerators;

namespace PFSP.Algorithms.SimulatedAnnealing
{
    public sealed class SimulatedAnnealingState : AlgorithmState
    {
        public SimulatedAnnealingState(
            Instance instance,
            SimulatedAnnealingParameters parameters,
            RandomPermutationSolutionGenerator generator,
            Random random)
            : base(instance, parameters)
        {
            Generator = generator;
            Random = random;
        }

        public new SimulatedAnnealingParameters Parameters => (SimulatedAnnealingParameters)base.Parameters;

        public RandomPermutationSolutionGenerator Generator { get; }

        public Random Random { get; }

        public PermutationSolution Current { get; set; } = null!;

        public PermutationSolution? Candidate { get; set; }

        public long Iteration { get; set; }

        public double Temperature { get; set; }

        public bool? Accepted { get; set; }
    }
}
