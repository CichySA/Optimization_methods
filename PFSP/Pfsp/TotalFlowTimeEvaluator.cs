using System;

namespace PFSP
{
    // Total flowtime evaluator (recursive with memoization)
    public class TotalFlowTimeEvaluator : IEvaluator
    {
        public double Evaluate(Instance instance, int[] permutation)
        {
            if (instance.Jobs != permutation.Length)
                throw new ArgumentException($"invalid permutation length, expected {instance.Jobs}, got {permutation.Length}");

            int machines = instance.Machines;
            int jobs = instance.Jobs;
            if (machines <= 0 || jobs <= 0)
                return 0.0;

            // memo[machine, position] where position is index in permutation (0..jobs-1)
            var memo = new double[machines, jobs];
            for (int m = 0; m < machines; m++)
                for (int p = 0; p < jobs; p++)
                    memo[m, p] = double.NaN;

            double CompletionTime(int machine, int pos)
            {
                if (!double.IsNaN(memo[machine, pos]))
                    return memo[machine, pos];

                // processing time for the job at position 'pos' on 'machine'
                double processingTime = instance.Matrix[machine, permutation[pos]];
                double result;

                if (machine == 0 && pos == 0)
                {
                    result = processingTime;
                }
                else if (machine == 0)
                {
                    result = processingTime + CompletionTime(0, pos - 1);
                }
                else if (pos == 0)
                {
                    result = processingTime + CompletionTime(machine - 1, 0);
                }
                else
                {
                    double t1 = CompletionTime(machine - 1, pos);
                    double t2 = CompletionTime(machine, pos - 1);
                    result = processingTime + Math.Max(t1, t2);
                }

                memo[machine, pos] = result;
                return result;
            }

            double timeFlow = 0.0;
            int lastMachine = machines - 1;
            for (int pos = 0; pos < jobs; pos++)
            {
                timeFlow += CompletionTime(lastMachine, pos);
            }

            return timeFlow;
        }
    }
}