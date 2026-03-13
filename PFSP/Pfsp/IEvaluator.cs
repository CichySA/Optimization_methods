namespace PFSP
{
    // IEvaluator defines different strategy of evaluation.
    public interface IEvaluator
    {
        double Evaluate(Instance instance, int[] permutation);
    }
}
