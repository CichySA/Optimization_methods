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
            ArgumentNullException.ThrowIfNull(solution);
            return Evaluate(instance, solution.Permutation);
        }

        public double Evaluate(Instance instance, int[] permutation)
        {
#if DEBUG || XUNIT
            ValidateInputs(instance, permutation);
#endif

            int machines = instance.Machines;
            int assigned = permutation.Length;
            if (machines <= 0 || instance.Jobs <= 0 || assigned == 0)
                return 0.0;

            double[] comp = ArrayPool<double>.Shared.Rent(assigned);
            try
            {
                double timeFlow = 0.0;
                int lastMachine = machines - 1;

                // m=0
                {
                    double left = 0.0;
                    for (int p = 0; p < assigned; p++)
                    {
                        double cur = instance.Matrix[0, permutation[p]] + left;
                        comp[p] = cur;
                        left = cur;
                    }
                }

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
            ArgumentNullException.ThrowIfNull(instance);
            ArgumentNullException.ThrowIfNull(permutation);
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