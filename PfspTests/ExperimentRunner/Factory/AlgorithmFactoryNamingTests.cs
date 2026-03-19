using ExperimentRunner;

namespace PfspTests.ExperimentRunner.Factory
{
    [Trait("Area", "ExperimentRunner")]
    [Trait("Component", "AlgorithmFactory")]
    [Trait("Kind", "Unit")]
    public class AlgorithmFactoryNamingTests
    {
        [Fact]
        public void CreateFromSpec_RandomName_ContainsSamplesAndSeed()
        {
            var spec = new AlgorithmSpec { Type = "Random", Parameters = AlgorithmFactoryTestData.Elem("""{ "Seed": 42, "Samples": 200 }""") };

            var (name, _, _) = AlgorithmFactory.CreateFromSpec(spec);

            Assert.Contains("200", name);
            Assert.Contains("42", name);
        }

        [Fact]
        public void CreateFromSpec_EvolutionaryName_ContainsPopulationGenerationsAndSeed()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Evolutionary",
                Parameters = AlgorithmFactoryTestData.Elem("""{ "PopulationSize": 50, "Generations": 10, "Seed": 1 }""")
            };

            var (name, _, _) = AlgorithmFactory.CreateFromSpec(spec);

            Assert.Contains("50", name);
            Assert.Contains("10", name);
            Assert.Contains("1", name);
        }
    }
}
