using PFSP.Instances;
using PFSP.Solutions;
using System.Buffers;

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
            // Rent a buffer from the pool to avoid a heap allocation on every call.
            // Array.Clear is intentionally avoided: the first machine pass (m=0) initialises
            // every comp[p] from scratch (up is implicitly 0 because there is no prior machine,
            // and processing times are non-negative so Math.Max(0, left) == left always).
            double[] comp = ArrayPool<double>.Shared.Rent(assigned);
            try
            {
                double timeFlow = 0.0;
                int lastMachine = machines - 1;

                // m=0: no previous machine, so C[-1,p] = 0 for all p.
                // Math.Max(0, left) == left (left starts at 0 and only grows), so skip the Max.
                {
                    double left = 0.0;
                    for (int p = 0; p < assigned; p++)
                    {
                        double cur = instance.Matrix[0, permutation[p]] + left;
                        comp[p] = cur;
                        left = cur;
                    }
                }

                // m=1..lastMachine-1: accumulate completion times without the branch on lastMachine.
                for (int m = 1; m < lastMachine; m++)
                {
                    double left = 0.0;
                    for (int p = 0; p < assigned; p++)
                    {
                        double cur = instance.Matrix[m, permutation[p]] + Math.Max(comp[p], left);
                        comp[p] = cur;
                        left = cur;
                    }
                }

                // last machine: same loop but also accumulate into timeFlow.
                if (lastMachine > 0)
                {
                    double left = 0.0;
                    for (int p = 0; p < assigned; p++)
                    {
                        double cur = instance.Matrix[lastMachine, permutation[p]] + Math.Max(comp[p], left);
                        comp[p] = cur;
                        left = cur;
                        timeFlow += cur;
                    }
                }
                else
                {
                    // single-machine case: m=0 was already the last machine
                    for (int p = 0; p < assigned; p++)
                        timeFlow += comp[p];
                }

                return timeFlow;
            }
            finally
            {
                ArrayPool<double>.Shared.Return(comp);
            }
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