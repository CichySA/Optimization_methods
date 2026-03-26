using PFSP.Monitoring;

namespace PFSP.Algorithms.Greedy
{
    public sealed class GreedyParameters : IParameters
    {
        public AlgorithmMonitoringOptions Monitoring { get; set; } = new();
    }
}
