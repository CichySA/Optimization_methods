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
                    var row = instance.GetMachineRow(0);
                    double left = 0.0;
                    for (int p = 0; p < assigned; p++)
                    {
                        double cur = row[permutation[p]] + left;
                        comp[p] = cur;
                        left = cur;
                    }
                }

                for (int m = 1; m < lastMachine; m++)
                {
                    var row = instance.GetMachineRow(m);
                    double left = 0.0;
                    for (int p = 0; p < assigned; p++)
                    {
                        double up = comp[p];
                        double cur = row[permutation[p]] + (up > left ? up : left);
                        comp[p] = cur;
                        left = cur;
                    }
                }

                if (lastMachine > 0)
                {
                    var row = instance.GetMachineRow(lastMachine);
                    double left = 0.0;
                    for (int p = 0; p < assigned; p++)
                    {
                        double up = comp[p];
                        double cur = row[permutation[p]] + (up > left ? up : left);
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
    }
}