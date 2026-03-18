using PFSP.Instances;
using PFSP.Solutions;
using System;
using System.Buffers;

namespace PFSP.Evaluators
{
    // Makespan evaluator
    public class MakespanEvaluator : IEvaluator
    {
        public double Evaluate(Instance instance, ISolution solution)
        {
            ArgumentNullException.ThrowIfNull(solution);
            return Evaluate(instance, solution.Permutation);
        }

        public double Evaluate(Instance instance, int[] permutation)
        {
#if DEBUG || XUNIT
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (permutation == null) throw new ArgumentNullException(nameof(permutation));
            if (permutation.Length == 0 || permutation.Length > instance.Jobs)
                throw new ArgumentException("invalid permutation length");
            for (int i = 0; i < permutation.Length; i++)
                if ((uint)permutation[i] >= (uint)instance.Jobs)
                    throw new ArgumentException("invalid permutation");
#endif

            int machines = instance.Machines;
            double[] timeTable = ArrayPool<double>.Shared.Rent(machines);
            Array.Clear(timeTable, 0, machines);
            try
            {
                foreach (var job in permutation)
                {
                    for (int machine = 0; machine < machines; machine++)
                    {
                        var processingTime = instance.Matrix[machine, job];
                        if (machine == 0)
                        {
                            timeTable[machine] += processingTime;
                        }
                        else
                        {
                            if (timeTable[machine - 1] < timeTable[machine])
                                timeTable[machine] += processingTime;
                            else
                                timeTable[machine] = timeTable[machine - 1] + processingTime;
                        }
                    }
                }

                return timeTable[machines - 1];
            }
            finally
            {
                ArrayPool<double>.Shared.Return(timeTable);
            }
        }
    }
}
