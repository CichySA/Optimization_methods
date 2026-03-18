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
        public int TournamentSize { get; set; } = DefaultTournamentSize;

        public ISelectionMethod SelectionMethod { get; set; } = new TournamentSelection();
        public ICrossoverMethod CrossoverMethod { get; set; } = new OrderCrossover();
        public IMutationMethod MutationMethod { get; set; } = new SwapMutation();

        public static EvolutionaryParameters Default => new();
    }

}
