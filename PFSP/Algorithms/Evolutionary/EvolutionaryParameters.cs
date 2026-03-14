namespace PFSP.Algorithms.Evolutionary
{
    public sealed class EvolutionaryParameters : IParameters
    {
        public int PopulationSize { get; set; } = 100;
        public int Generations { get; set; } = 1000;
        public double CrossoverRate { get; set; } = 0.9;
        public double MutationRate { get; set; } = 0.02;
        public int TournamentSize { get; set; } = 3;
    }
}
