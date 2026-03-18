using PFSP.Solutions;

namespace PfspTests
{
    public class PermutationSolutionTests
    {
        [Fact]
        public void CreateCopy_IsIndependentOfOriginalArray()
        {
            int[] original = [0, 1, 2, 3];
            var solution = PermutationSolution.CreateCopy(original, 5.0);

            original[0] = 99;

            Assert.Equal(0, solution.Permutation[0]);
        }
    }
}
