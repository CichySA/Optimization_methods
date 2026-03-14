using PFSP.Instances;
using PFSP.Solutions;

namespace PFSP.Evaluators
{
    // Total flowtime evaluator
    public class TotalFlowTimeEvaluator : IEvaluator
    {
        public double Evaluate(Instance instance, ISolution solution)
        {
#if DEBUG || XUNIT
            ValidateInputs(instance, solution.Permutation);
#endif

            int machines = instance.Machines;
            int jobs = instance.Jobs;
            int[] permutation = solution.Permutation;    
            int assigned = permutation.Length;
            if (machines <= 0 || jobs <= 0)
                return 0.0;

            // dynamic programming
            // `comp[p]` holds C[m-1,p] (previous machine) until overwritten.
            var comp = new double[assigned];
            double timeFlow = 0.0;
            int lastMachine = machines - 1;

            for (int m = 0; m < machines; m++)
            {
                double left = 0.0; // C[m, p-1]
                for (int p = 0; p < assigned; p++)
                {
                    double up = comp[p]; // C[m-1, p]
                    double processingTime = instance.Matrix[m, permutation[p]];

                    double cur = processingTime + Math.Max(up, left);
                    comp[p] = cur; // becomes C[m, p] for next machine
                    left = cur;

                    if (m == lastMachine)
                        timeFlow += cur;
                }
            }

            return timeFlow;
        }

        private static void ValidateInputs(Instance instance, int[] permutation)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (permutation == null)
                throw new ArgumentNullException(nameof(permutation));
            if (instance.Jobs == 0 || permutation.Length == 0 || permutation.Length > instance.Jobs)
                throw new ArgumentException($"invalid permutation");
            for (int i = 0; i < permutation.Length; i++)
            {
                if ((uint)permutation[i] >= (uint)instance.Jobs)
                    throw new ArgumentException($"invalid permutation");
            }
        }
    }
}