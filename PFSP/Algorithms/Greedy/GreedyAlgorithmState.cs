using PFSP.Algorithms.Monitoring;
using PFSP.Instances;
using PFSP.Solutions;
using System.Diagnostics;

namespace PFSP.Algorithms.Greedy
{
    public sealed class GreedyAlgorithmState : AlgorithmState
    {
        public GreedyAlgorithmState(
            Instance instance,
            GreedyParameters parameters,
            Stopwatch stopwatch)
            : base(instance, parameters, stopwatch)
        {
        }

        public new GreedyParameters Parameters => (GreedyParameters)base.Parameters;

        public PermutationSolution? Candidate { get; set; }

        public long Step { get; set; }
    }
}
