using PFSP.Algorithms.Monitoring;
using PFSP.Instances;
using PFSP.Solutions;
using PFSP.Solutions.PermutationSolutionGenerators;

namespace PFSP.Algorithms.RandomSearch
{
    public sealed class RandomSearchState : AlgorithmState
    {
        private readonly Func<RandomSearchState, bool> _continueCondition;

        public RandomSearchState(
            Instance instance,
            RandomSearchParameters parameters)
            : base(instance, parameters)
        {
            RemainingIterations = parameters.Samples;
            Generator = new RandomPermutationSolutionGenerator(parameters.Seed);
            CandidatePermutation = new int[instance.Jobs];
            CandidateBuffer = PermutationSolution.WrapBuffer(CandidatePermutation, 0.0);
            _continueCondition = parameters.TimeLimit.HasValue
                ? static state => state.Elapsed < state.Parameters.TimeLimit!.Value
                : static state => state.RemainingIterations-- > 0;
        }

        public new RandomSearchParameters Parameters => (RandomSearchParameters)base.Parameters;

        public long RemainingIterations { get; set; }

        public RandomPermutationSolutionGenerator Generator { get; }

        public int[] CandidatePermutation { get; }

        public PermutationSolution CandidateBuffer { get; }

        public PermutationSolution? Candidate { get; set; }

        public bool ShouldContinue() => _continueCondition(this);
    }
}
