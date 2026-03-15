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
    }
}
