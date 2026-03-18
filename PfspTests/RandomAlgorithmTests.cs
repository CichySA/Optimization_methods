using PFSP.Algorithms;
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

        [Fact]
        public void Solve_PerformsExpectedNumberOfEvaluations_WithRandom()
        {
            int samples = 1000; // expected number of evaluations
            int seed = 42;      // deterministic RNG for test repeatability

            // only change the instantiated algorithm
            IAlgorithm alg = new RandomAlgorithm();
            RunEvaluationCountTest(alg, samples, seed);
        }

        [Fact]
        public void Solve_PerformsExpectedNumberOfEvaluations_WithRandomParallel()
        {
            int samples = 1000; // expected number of evaluations
            int seed = 42;      // deterministic RNG for test repeatability

            // only change the instantiated algorithm
            IAlgorithm alg = new ParallelRandomAlgorithm();
            RunEvaluationCountTest(alg, samples, seed);
        }

        private void RunEvaluationCountTest(IAlgorithm alg, int samples, int seed)
        {
            // Arrange
            int machines = 3;
            int jobs = 10;
            double[,] matrix = new double[machines, jobs];

            // Fill matrix with small deterministic values so Evaluate is cheap.
            var r = new Random(123);
            for (int m = 0; m < machines; m++)
                for (int j = 0; j < jobs; j++)
                    matrix[m, j] = r.NextDouble() * 10.0;

            var instance = Instance.CreateWithDefaultEvaluator(matrix);

            var parameters = RandomParameters.ForRuns(samples, seed);

            // Act
            var result = alg.Solve(instance, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Best);
            Assert.Equal((long)samples, result.Evaluations);
        }

        // --- Group: Sequential guards ---

        [Fact]
        public void Sequential_Solve_NullInstance_ThrowsArgumentNullException()
        {
            var algo = new RandomAlgorithm();
            var pars = RandomParameters.ForRuns(10, seed: 1);
            Assert.Throws<ArgumentNullException>(() => algo.Solve(null!, pars));
        }

        [Fact]
        public void Sequential_Solve_WrongParameterType_ThrowsArgumentException()
        {
            var matrix = new double[1, 2] { { 1, 2 } };
            var instance = Instance.Create(matrix, new TotalFlowTimeEvaluator());
            var algo = new RandomAlgorithm();
            Assert.Throws<ArgumentException>(() => algo.Solve(instance, new FakeParameters()));
        }

        // --- Group: Sequential functional correctness (TODO items) ---

        [Fact]
        public void Sequential_DifferentSeeds_ProduceDifferentResults()
        {
            // Use 20 jobs so the search space (20!) is too large for limited samples
            // to converge to the same best with different seeds.
            int jobs = 20;
            var matrix = new double[3, jobs];
            var rnd = new Random(0);
            for (int m = 0; m < 3; m++)
                for (int j = 0; j < jobs; j++)
                    matrix[m, j] = rnd.Next(1, 100);
            var instance = Instance.Create(matrix, new TotalFlowTimeEvaluator());
            var algo = new RandomAlgorithm();

            var res1 = algo.Solve(instance, RandomParameters.ForRuns(10, seed: 1), TestContext.Current.CancellationToken);
            var res2 = algo.Solve(instance, RandomParameters.ForRuns(10, seed: 2), TestContext.Current.CancellationToken);

            Assert.False(res1.Best.Permutation.SequenceEqual(res2.Best.Permutation));
        }

        [Fact]
        public void Sequential_Solve_ReturnsValidPermutation()
        {
            int jobs = 7;
            var matrix = new double[2, jobs];
            for (int j = 0; j < jobs; j++) { matrix[0, j] = j + 1; matrix[1, j] = jobs - j; }
            var instance = Instance.Create(matrix, new TotalFlowTimeEvaluator());
            var algo = new RandomAlgorithm();

            var result = algo.Solve(instance, RandomParameters.ForRuns(50, seed: 42), TestContext.Current.CancellationToken);

            Assert.Equal(jobs, result.Best.Permutation.Length);
            Assert.Equal(Enumerable.Range(0, jobs), result.Best.Permutation.OrderBy(x => x));
        }

        [Fact]
        public void Sequential_Solve_WithTimeLimit_ReturnsAtLeastOneEvaluation()
        {
            var matrix = new double[2, 5] { { 1, 2, 3, 4, 5 }, { 5, 4, 3, 2, 1 } };
            var instance = Instance.Create(matrix, new TotalFlowTimeEvaluator());
            var algo = new RandomAlgorithm();

            var result = algo.Solve(instance, RandomParameters.ForTimeLimit(TimeSpan.FromMilliseconds(100), seed: 1), TestContext.Current.CancellationToken);

            Assert.NotNull(result.Best);
            Assert.True(result.Evaluations >= 1);
        }

        // --- Group: Parallel correctness ---

        [Fact]
        public void Parallel_FixedSeed_IsDeterministic()
        {
            var matrix = new double[2, 5]
            {
                { 1, 2, 3, 4, 5 },
                { 5, 4, 3, 2, 1 }
            };
            var instance = Instance.Create(matrix, new TotalFlowTimeEvaluator());
            var algo = new ParallelRandomAlgorithm();
            var pars = RandomParameters.ForRuns(100, seed: 123);

            var res1 = algo.Solve(instance, pars, TestContext.Current.CancellationToken);
            var res2 = algo.Solve(instance, pars, TestContext.Current.CancellationToken);

            Assert.Equal(res1.Best.Cost, res2.Best.Cost);
            Assert.Equal(res1.Best.Permutation, res2.Best.Permutation);
        }

        [Fact]
        public void Parallel_Solve_ReturnsValidPermutation()
        {
            int jobs = 7;
            var matrix = new double[2, jobs];
            for (int j = 0; j < jobs; j++) { matrix[0, j] = j + 1; matrix[1, j] = jobs - j; }
            var instance = Instance.Create(matrix, new TotalFlowTimeEvaluator());
            var algo = new ParallelRandomAlgorithm();

            var result = algo.Solve(instance, RandomParameters.ForRuns(50, seed: 42), TestContext.Current.CancellationToken);

            Assert.Equal(jobs, result.Best.Permutation.Length);
            Assert.Equal(Enumerable.Range(0, jobs), result.Best.Permutation.OrderBy(x => x));
        }

        // --- Group: Parallel guards ---

        [Fact]
        public void Parallel_Solve_NullInstance_ThrowsArgumentNullException()
        {
            var algo = new ParallelRandomAlgorithm();
            var pars = RandomParameters.ForRuns(10, seed: 1);
            Assert.Throws<ArgumentNullException>(() => algo.Solve(null!, pars));
        }

        [Fact]
        public void Parallel_Solve_WrongParameterType_ThrowsArgumentException()
        {
            var matrix = new double[1, 2] { { 1, 2 } };
            var instance = Instance.Create(matrix, new TotalFlowTimeEvaluator());
            var algo = new ParallelRandomAlgorithm();
            Assert.Throws<ArgumentException>(() => algo.Solve(instance, new FakeParameters()));
        }

        [Fact]
        public void Parallel_Solve_CancellationRequested_StopsGracefully()
        {
            var matrix = new double[2, 10];
            for (int j = 0; j < 10; j++) { matrix[0, j] = j + 1; matrix[1, j] = 10 - j; }
            var instance = Instance.Create(matrix, new TotalFlowTimeEvaluator());
            var algo = new ParallelRandomAlgorithm();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Pre-cancelled token: algorithm must not throw and must return a valid fallback solution.
            var result = algo.Solve(instance, RandomParameters.ForRuns(1_000_000, seed: 1), cts.Token);

            Assert.NotNull(result.Best);
            Assert.Equal(instance.Jobs, result.Best.Permutation.Length);
        }

        private sealed class FakeParameters : IParameters { }
    }
}

