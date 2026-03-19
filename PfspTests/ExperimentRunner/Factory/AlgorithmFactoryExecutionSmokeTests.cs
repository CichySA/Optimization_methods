using ExperimentRunner;

namespace PfspTests.ExperimentRunner.Factory
{
    [Trait("Area", "ExperimentRunner")]
    [Trait("Component", "AlgorithmFactory")]
    [Trait("Kind", "Smoke")]
    public class AlgorithmFactoryExecutionSmokeTests
    {
        [Fact]
        public void CreateFromSpec_Random_SolvesInstance_ReturnsBestSolution()
        {
            var spec = new AlgorithmSpec { Type = "Random", Parameters = AlgorithmFactoryTestData.Elem("""{ "Seed": 1, "Samples": 20 }""") };
            var (_, algo, pars) = AlgorithmFactory.CreateFromSpec(spec);

            var result = algo.Solve(AlgorithmFactoryTestData.SmallInstance(), pars, TestContext.Current.CancellationToken);

            Assert.NotNull(result.Best);
            Assert.True(result.Best.Cost > 0);
            Assert.Equal(20, result.Evaluations);
        }

        [Fact]
        public void CreateFromSpec_Evolutionary_SolvesInstance_ReturnsBestSolution()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Evolutionary",
                Parameters = AlgorithmFactoryTestData.Elem("""{ "Seed": 1, "PopulationSize": 10, "Generations": 5 }""")
            };
            var (_, algo, pars) = AlgorithmFactory.CreateFromSpec(spec);

            var result = algo.Solve(AlgorithmFactoryTestData.SmallInstance(), pars, TestContext.Current.CancellationToken);

            Assert.NotNull(result.Best);
            Assert.True(result.Best.Cost > 0);
        }

        [Fact]
        public void CreateFromSpec_Greedy_SolvesInstance_ReturnsBestSolution()
        {
            var spec = new AlgorithmSpec { Type = "Greedy" };
            var (_, algo, pars) = AlgorithmFactory.CreateFromSpec(spec);

            var result = algo.Solve(AlgorithmFactoryTestData.SmallInstance(), pars, TestContext.Current.CancellationToken);

            Assert.NotNull(result.Best);
            Assert.True(result.Best.Cost > 0);
        }
    }
}
