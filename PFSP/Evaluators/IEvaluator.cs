using PFSP.Instances;
using PFSP.Solutions;

namespace PFSP.Evaluators
{
    // IEvaluator defines different strategy of evaluation.
    public interface IEvaluator
    {
        // Primary evaluation entry accepting a solution object.
        double Evaluate(Instance instance, ISolution solution)
        {
            ArgumentNullException.ThrowIfNull(instance);
            ArgumentNullException.ThrowIfNull(solution);
            return Evaluate(instance, solution.Permutation);
        }

        // Evaluate a permutation provided as an owned array. Implementations
        // should provide this overload as the primary array-backed evaluation
        // entry point.
        double Evaluate(Instance instance, int[] permutation);
    }
}
