using PFSP.Algorithms.Monitoring;
using PFSP.Instances;
using PFSP.Solutions;
using PFSP.Solutions.PermutationSolutionGenerators;

namespace PFSP.Algorithms.Evolutionary
{
    public sealed class EvolutionaryAlgorithmState : AlgorithmState
    {
        public EvolutionaryAlgorithmState(
            Instance instance,
            EvolutionaryParameters parameters,
            RandomPermutationSolutionGenerator generator,
            Random random)
            : base(instance, parameters)
        {
            Generator = generator;
            Random = random;
            PopulationSize = Math.Max(1, parameters.PopulationSize);
            Population = new PermutationSolution[PopulationSize];
            NextPopulation = new PermutationSolution[PopulationSize];
        }

        public new EvolutionaryParameters Parameters => (EvolutionaryParameters)base.Parameters;

        public RandomPermutationSolutionGenerator Generator { get; }

        public Random Random { get; }

        public int PopulationSize { get; }

        public PermutationSolution[] Population { get; set; }

        public PermutationSolution[] NextPopulation { get; set; }

        public PermutationSolution? Candidate { get; set; }

        public long Generation { get; set; }
    }
}
