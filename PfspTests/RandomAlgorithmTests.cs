using PFSP.Algorithms.Random;
using PFSP.Evaluators;
using PFSP.Instances;

namespace PfspTests
{
    public class RandomAlgorithmTests
    {
        //@TODO use tai
        //TODO test randomness
        //TODO test that different seeds give different results
        //TODO test that the AlgorithmSolution is valid (permutation of jobs)
        [Fact]
        public void FixedSeed_IsDeterministic()
        {
            var matrix = new double[2, 5]
            {
                {1,2,3,4,5},
                {5,4,3,2,1}
            };
            var instance = Instance.Create(matrix, new TotalFlowTimeEvaluator());

            var algo = new RandomAlgorithm();
            var pars = RandomParameters.ForRuns(100, seed: 123);

            var res1 = algo.Solve(instance, pars, TestContext.Current.CancellationToken);
            var res2 = algo.Solve(instance, pars, TestContext.Current.CancellationToken);

            // With same seed and parameters the best cost should be equal
            Assert.Equal(res1.Best.Cost, res2.Best.Cost);
            Assert.Equal(res1.Best.Permutation, res2.Best.Permutation);
        }
    }
}
