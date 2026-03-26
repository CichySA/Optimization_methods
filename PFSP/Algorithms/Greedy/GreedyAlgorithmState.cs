using PFSP.Instances;
using PFSP.Solutions;

namespace PFSP.Algorithms.Greedy
{
    public sealed class GreedyAlgorithmState : AlgorithmState
    {
        public GreedyAlgorithmState(
            Instance instance,
            GreedyParameters parameters)
            : base(instance, parameters)
        {
        }

        public new GreedyParameters Parameters => (GreedyParameters)base.Parameters;

        public PermutationSolution? Candidate { get; set; }

        public long Step { get; set; }
    }
}
