using PFSP.Solutions;

namespace PfspTests
{
    public class TotalFlowTimeTests
    {
        [Fact]
        public void Evaluate_SmallInstance_ComputesExpectedTotalFlowTime()
        {
            // Construct a small instance with 2 machines and 3 jobs
            // Machine 0 processing times: [1,2,3]
            // Machine 1 processing times: [4,5,6]
            var matrix = new double[2, 3]
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };
            var instance = PFSP.Instances.Instance.Create(matrix, new PFSP.Evaluators.TotalFlowTimeEvaluator());

            // permutation [0,1,2]
            int[] permutation = [0, 1, 2];
            var solution = PermutationSolution.CreateCopy(permutation, 0);

            var result = instance.Evaluate(solution);

            // Manually computed total flow time for this instance and permutation is 31
            Assert.Equal(31.0, result, 10);
        }

        [Fact]
        public void Evaluate_SingleMachine_ComputesExpectedTotalFlowTime()
        {
            // 1 machine, 3 jobs with times [3, 1, 4]; perm [0,1,2]
            // Completion times: 3, 3+1=4, 4+4=8 → TFT = 3+4+8 = 15
            var matrix = new double[1, 3] { { 3, 1, 4 } };
            var instance = PFSP.Instances.Instance.Create(matrix, new PFSP.Evaluators.TotalFlowTimeEvaluator());
            var solution = PermutationSolution.CreateCopy([0, 1, 2], 0);

            Assert.Equal(15.0, instance.Evaluate(solution), 10);
        }

        [Fact]
        public void Evaluate_DifferentPermutations_GiveDifferentCosts()
        {
            // perm [0,1,2] → TFT=31, perm [2,1,0] → TFT=41 (manually verified)
            var matrix = new double[2, 3]
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };
            var instance = PFSP.Instances.Instance.Create(matrix, new PFSP.Evaluators.TotalFlowTimeEvaluator());

            var cost1 = instance.Evaluate(PermutationSolution.CreateCopy([0, 1, 2], 0));
            var cost2 = instance.Evaluate(PermutationSolution.CreateCopy([2, 1, 0], 0));

            Assert.NotEqual(cost1, cost2);
        }
    }
}
