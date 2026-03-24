using PFSP.Algorithms.Greedy;
using PFSP.Instances;
using PFSP.Solutions;
using PFSP.Evaluators;
using PFSP.Algorithms;

namespace PfspTests.Algorithms.Greedy
{
    public class GreedyAlgorithmTests
    {
        [Fact]
        public void Solve_NullInstance_ThrowsArgumentNullException()
        {
            var algo = new GreedyAlgorithm();
            var pars = new GreedyParameters();
            Assert.Throws<ArgumentNullException>(() => algo.Solve(null!, pars));
        }

        [Fact]
        public void Solve_WrongParameters_ThrowsArgumentException()
        {
            var matrix = new double[1, 2] { { 1, 2 } };
            var instance = Instance.Create(matrix, new TotalFlowTimeEvaluator());
            var algo = new GreedyAlgorithm();
            Assert.Throws<ArgumentException>(() => algo.Solve(instance, new FakeParameters()));
        }

        [Fact]
        public void Solve_ReturnsValidCompleteSolution()
        {
            // Simple 3-job, 2-machine instance
            var matrix = new double[2, 3]
            {
                { 2, 3, 4 },
                { 5, 2, 3 }
            };
            var instance = Instance.Create(matrix, new TotalFlowTimeEvaluator());

            var algo = new GreedyAlgorithm();
            var pars = new GreedyParameters();

            var result = algo.Solve(instance, pars);

            // solution must return a result
            Assert.NotNull(result);
            // solution must return a best solution
            Assert.NotNull(result.Best);
            // solution must be complete
            Assert.Equal(instance.Jobs, result.Best.Permutation.Length);
            // solution must be a permutation of unique jobs
            Assert.Equal(Enumerable.Range(0, instance.Jobs), result.Best.Permutation.OrderBy(x => x));
            // cost must match the instance evaluation
            var cost = instance.Evaluate(result.Best.Permutation);
            Assert.Equal(cost, result.Best.Cost);
            // expected deterministic value for this fixture
            Assert.Equal(28, cost);
            // no-search greedy (SPT by total processing time)
            Assert.Equal(new int[] { 1, 0, 2 }, result.Best.Permutation);
        }

        [Fact]
        public void SptAlgorithm_ReturnsValidCompleteSolution()
        {
            var matrix = new double[2, 3]
            {
                { 2, 3, 4 },
                { 5, 2, 3 }
            };
            var instance = Instance.Create(matrix, new TotalFlowTimeEvaluator());

            var algo = new SptAlgorithm();
            var pars = new GreedyParameters();

            var result = algo.Solve(instance, pars);

            Assert.NotNull(result);
            Assert.NotNull(result.Best);
            Assert.Equal(instance.Jobs, result.Best.Permutation.Length);
            Assert.Equal(Enumerable.Range(0, instance.Jobs), result.Best.Permutation.OrderBy(x => x));

            var cost = instance.Evaluate(result.Best.Permutation);
            Assert.Equal(cost, result.Best.Cost);
            Assert.Equal(28, cost);
            // iterative insertion variant reproduces previous behavior on this fixture
            Assert.Equal(new int[] { 0, 1, 2 }, result.Best.Permutation);
        }

        private sealed class FakeParameters : IParameters { }
    }
}
