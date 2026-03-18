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

        [Fact]
        public void Evaluate_PartialSolution_ComparedToCompleteSolution_ShowsProgression()
        {
            // Arrange: Create a 3-job, 2-machine instance to show how partial solutions relate to complete ones
            var matrix = new double[2, 3]
            {
                { 2, 3, 4 },  // Machine 0: jobs [0, 1, 2]
                { 5, 2, 3 }   // Machine 1: jobs [0, 1, 2]
            };

            var evaluator = new PFSP.Evaluators.TotalFlowTimeEvaluator();
            var instance = PFSP.Instances.Instance.Create(matrix, evaluator);

            // Act: Evaluate progressively larger partial permutations [0, 1, 2]
            var partial1Job = PermutationSolution.CreateCopy([0], 0.0);
            var partial2Jobs = PermutationSolution.CreateCopy([0, 1], 0.0);
            var fullPermutation = PermutationSolution.CreateCopy([0, 1, 2], 0.0);

            double tft1 = instance.Evaluate(partial1Job);
            double tft2 = instance.Evaluate(partial2Jobs);
            double tftFull = instance.Evaluate(fullPermutation);

            // Assert: Each additional job increases the total flow time
            // Partial [0]: Job0 completes at M0: 0 + 2 = 2, then at M1: 2 + 5 = 7 → TFT = 7
            Assert.Equal(7.0, tft1);
            
            // Partial [0,1]: Job0→M1: 7, Job1→M1: max(Job0->M1, Job1->M0) + 2 = max(7, 5) + 2 = 9 → TFT = 7 + 9 = 16
            Assert.Equal(16.0, tft2);
            
            // Full [0,1,2]: adds Job2 completion → TFT = 16 + (Job2 M1 completion)
            // Job2→M0: 5 + 4 = 9, Job2→M1: max(16, 9) + 3 = 19 → TFT = 19 + 9 = 28
            Assert.Equal(28.0, tftFull);

            // Verify progression
            Assert.True(tft1 < tft2);
            Assert.True(tft2 < tftFull);
        }
    }
}
