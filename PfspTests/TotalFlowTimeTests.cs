using PFSP;
using PFSP.Evaluators;
using PFSP.Instances;
using Xunit;
using PFSP.Solutions;

namespace PfspTests
{
    public class TotalFlowTimeTests
    {
        [Fact]
        public void Tai4_5_1_Sequence_1_3_4_2_Returns_1170()
        {
            // Build instance for tai4_5_1.fsp (machines x jobs)
            var instance = InstanceReader.Read(4,5,1);


            // sequence ⟨1,3,4,2⟩ is 1-based; convert to zero-based indices: [0,2,3,1]
            int[] permutation = [0, 2, 3, 1];
            PermutationSolution solution = new(permutation, 0);

            instance.Evaluator = new TotalFlowTimeEvaluator();

            var result = instance.Evaluate(solution);

            // expected total flowtime (sum of completion times on last machine) = 1170
            Assert.Equal(1170.0, result, 3);
        }
    }
}