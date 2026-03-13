using PFSP;
using Xunit;

namespace PfspTests
{
    // Reference total-flowtime evaluator (correct algorithm).
    class ReferenceTotalFlowTimeEvaluator : IEvaluator
    {
        public double Evaluate(Instance instance, int[] permutation)
        {
            if (instance.Jobs != permutation.Length)
                throw new System.ArgumentException($"invalid permutation length, expected {instance.Jobs}, got {permutation.Length}");

            var timeTable = new double[instance.Machines];
            double totalFlow = 0.0;

            foreach (var job in permutation)
            {
                for (int m = 0; m < instance.Machines; m++)
                {
                    var p = instance.Matrix[m, job];
                    if (m == 0)
                        timeTable[m] += p;
                    else
                    {
                        if (timeTable[m] < timeTable[m - 1])
                            timeTable[m] += p;
                        else
                            timeTable[m] = timeTable[m - 1] + p;
                    }
                }
                totalFlow += timeTable[instance.Machines - 1];
            }

            return totalFlow;
        }
    }

    public class TotalFlowTimeTests
    {
        [Fact]
        public void Tai4_5_1_Sequence_1_3_4_2_Returns_1170()
        {
            // Build instance for tai4_5_1.fsp (machines x jobs)
            var instance = InstanceReader.Read(4,5,1);


            // sequence ⟨1,3,4,2⟩ is 1-based; convert to zero-based indices: [0,2,3,1]
            int[] permutation = [0, 2, 3, 1];

            instance.Evaluator = new TotalFlowTimeEvaluator();

            var result = instance.Evaluate(permutation);

            // expected total flowtime (sum of completion times on last machine) = 1170
            Assert.Equal(1170.0, result, 3);
        }
    }
}