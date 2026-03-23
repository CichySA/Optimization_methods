using PFSP.Algorithms.Evolutionary.Operators;
using PFSP.Algorithms.Evolutionary.Operators.CrossoverOperators;
using PFSP.Algorithms.Evolutionary.Operators.MutationOperators;
using PFSP.Algorithms.Evolutionary.Operators.SelectionOperators;
using PFSP.Algorithms.Monitoring;

namespace PFSP.Algorithms.Evolutionary
{
    public sealed class EvolutionaryParameters : IParameters
    {
        public const int DefaultSeed = 0;
        public const int DefaultPopulationSize = 100;
        public const int DefaultGenerations = 100;
        public const double DefaultCrossoverRate = 0.7;
        public const double DefaultMutationRate = 0.1;
        public const int DefaultTournamentSize = 5;

        public int Seed { get; set; } = DefaultSeed;
        public int PopulationSize { get; set; } = DefaultPopulationSize;
        public int Generations { get; set; } = DefaultGenerations;
        public double CrossoverRate { get; set; } = DefaultCrossoverRate;
        public double MutationRate { get; set; } = DefaultMutationRate;
        public AlgorithmMonitoringOptions Monitoring { get; set; } = new();

        public ISelectionParameters SelectionParameters { get; set; }
            = new TournamentSelectionParameters { TournamentSize = DefaultTournamentSize };

        public int TournamentSize
        {
            get => SelectionParameters is TournamentSelectionParameters p ? p.TournamentSize : DefaultTournamentSize;
            set => SelectionParameters = new TournamentSelectionParameters { TournamentSize = value };
        }

        public ISelectionMethod SelectionMethod { get; set; } = new TournamentSelection();
        public ICrossoverMethod CrossoverMethod { get; set; } = new OrderCrossover();
        public IMutationMethod MutationMethod { get; set; } = new SwapMutation();

        public static EvolutionaryParameters Default => new();
    }
}
