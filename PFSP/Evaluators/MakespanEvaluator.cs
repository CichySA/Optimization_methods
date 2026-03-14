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
            if (instance.Jobs != solution.Permutation.Length)
                throw new ArgumentException($"invalid permutation length, expected {instance.Jobs}, got {solution.Permutation.Length}");

            int machines = instance.Machines;
            double[] timeTable = ArrayPool<double>.Shared.Rent(machines);
            Array.Clear(timeTable, 0, machines);
            try
            {
                foreach (var job in solution.Permutation)
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
