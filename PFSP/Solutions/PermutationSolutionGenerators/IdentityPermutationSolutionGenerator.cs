using PFSP.Instances;

namespace PFSP.Solutions.PermutationSolutionGenerators
{
    /// <summary>
    /// Returns the identity permutation [0, 1, ..., n-1] as a baseline starting solution.
    /// </summary>
    public sealed class IdentityPermutationSolutionGenerator : IPermutationSolutionGenerator
    {
        // Name removed; generators now only expose Create

        public PermutationSolution Create(Instance instance)
        {
            ArgumentNullException.ThrowIfNull(instance);

            int n = instance.Jobs;
            var perm = new int[n];
            for (int i = 0; i < n; i++) perm[i] = i;

            double cost = instance.Evaluate(perm);
            return new PermutationSolution(perm, cost);
        }
    }
}
