namespace PFSP.Algorithms.Greedy
{
    public sealed class GreedyParameters : IParameters
    {
        // For constructive greedy algorithms you may control randomness and tie breaking
        public int Seed { get; set; } = Environment.TickCount;
        public bool RandomTieBreak { get; set; } = false;
    }
}
