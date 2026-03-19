using PFSP.Algorithms.Evolutionary;
using PFSP.Instances;

namespace PfspTests
{
    public class EvolutionaryAlgorithmTests
    {
        [Fact]
        public void FixedSeed_OnTai20_5_0_IsDeterministic_AndReturnsValidPermutation()
        {
            var instance = InstanceReader.Read("tai20_5_0");

            var parameters = EvolutionaryParameters.Default;
            parameters.Seed = 12345;
            parameters.PopulationSize = 50;
            parameters.Generations = 50;

            var algorithm = new EvolutionaryAlgorithm();

            var result1 = algorithm.Solve(instance, parameters, TestContext.Current.CancellationToken);
            var result2 = algorithm.Solve(instance, parameters, TestContext.Current.CancellationToken);

            // Check that both runs produce the same best solution and that it's valid
            Assert.NotNull(result1.Best);
            Assert.NotNull(result2.Best);
            Assert.Equal(result1.Best.Cost, result2.Best.Cost);
            Assert.Equal(result1.Best.Permutation, result2.Best.Permutation);
            Assert.Equal(instance.Jobs, result1.Best.Permutation.Length);

            Assert.Equal(Enumerable.Range(0, instance.Jobs), result1.Best.Permutation.OrderBy(x => x));
            Assert.Equal((long)parameters.PopulationSize * (parameters.Generations + 1), result1.Evaluations);
            Assert.True(result1.BestFoundAtEvaluation > 0);
            Assert.True(result1.BestFoundAtEvaluation <= result1.Evaluations);
        }
    }
}
