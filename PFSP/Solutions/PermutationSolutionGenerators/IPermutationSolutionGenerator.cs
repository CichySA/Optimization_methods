using PFSP.Instances;
using PFSP.Solutions;

namespace PFSP.Solutions.PermutationSolutionGenerators
{
    /// <summary>
    /// Generates a fully evaluated candidate solution for a given instance.
    /// Each implementation owns its own state (e.g. RNG) and is configured at construction time.
    /// </summary>
    public interface IPermutationSolutionGenerator
    {
        PermutationSolution Create(Instance instance);
    }
}
