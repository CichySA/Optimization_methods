using System;

namespace PFSP
{
    // Makespan evaluator
    public class MakespanEvaluator : IEvaluator
    {
        public double Evaluate(Instance instance, int[] permutation)
        {
            if (instance.Jobs != permutation.Length)
                throw new ArgumentException($"invalid permutation length, expected {instance.Jobs}, got {permutation.Length}");
            
            var timeTable = new double[instance.Machines];
            for (int i = 0; i < instance.Machines; i++) timeTable[i] = 0.0;

            foreach (var job in permutation)
            {
                for (int machine = 0; machine < instance.Machines; machine++)
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

            return timeTable[instance.Machines - 1];
        }
    }
}
