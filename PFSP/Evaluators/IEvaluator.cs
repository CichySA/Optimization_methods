using PFSP.Instances;
using PFSP.Solutions;

namespace PFSP.Evaluators
{
    // IEvaluator defines different strategy of evaluation.
    public interface IEvaluator
    {
        double Evaluate(Instance instance, ISolution solution);
    }
}
