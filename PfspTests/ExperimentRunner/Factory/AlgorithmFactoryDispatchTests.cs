using ExperimentRunner;
using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.Greedy;
using PFSP.Algorithms.RandomSearch;

namespace PfspTests.ExperimentRunner.Factory
{
    [Trait("Area", "ExperimentRunner")]
    [Trait("Component", "AlgorithmFactory")]
    [Trait("Kind", "Unit")]
    public class AlgorithmFactoryDispatchTests
    {
        [Fact]
        public void CreateFromSpec_RandomType_ReturnsRandomAlgorithmWithCorrectParameters()
        {
            var spec = new AlgorithmSpec { Type = "Random", Parameters = AlgorithmFactoryTestData.Elem("""{ "Seed": 7, "Samples": 50 }""") };

            var (name, algo, pars) = AlgorithmFactory.CreateFromSpec(spec);

            Assert.Contains("Random", name);
            Assert.IsType<RandomSearchAlgorithm>(algo);
            var rp = Assert.IsType<RandomSearchParameters>(pars);
            Assert.Equal(7, rp.Seed);
            Assert.Equal(50, rp.Samples);
        }

        [Fact]
        public void CreateFromSpec_EvolutionaryType_ReturnsEvolutionaryAlgorithmWithCorrectParameters()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Evolutionary",
                Parameters = AlgorithmFactoryTestData.Elem("""{ "PopulationSize": 30, "Generations": 5, "Seed": 3 }""")
            };

            var (name, algo, pars) = AlgorithmFactory.CreateFromSpec(spec);

            Assert.Contains("Evolutionary", name);
            Assert.IsType<EvolutionaryAlgorithm>(algo);
            var ep = Assert.IsType<EvolutionaryParameters>(pars);
            Assert.Equal(30, ep.PopulationSize);
            Assert.Equal(5, ep.Generations);
            Assert.Equal(3, ep.Seed);
        }

        [Fact]
        public void CreateFromSpec_GreedyType_ReturnsGreedyAlgorithm()
        {
            var spec = new AlgorithmSpec { Type = "Greedy" };

            var (name, algo, pars) = AlgorithmFactory.CreateFromSpec(spec);

            Assert.Equal("Greedy", name);
            Assert.IsType<GreedyAlgorithm>(algo);
            Assert.IsType<GreedyParameters>(pars);
        }

        [Fact]
        public void CreateFromSpec_SptType_ReturnsSptAlgorithm()
        {
            var spec = new AlgorithmSpec { Type = "SPT" };

            var (name, algo, pars) = AlgorithmFactory.CreateFromSpec(spec);

            Assert.Equal("SPT", name);
            Assert.IsType<SptAlgorithm>(algo);
            Assert.IsType<GreedyParameters>(pars);
        }

        [Theory]
        [InlineData("random")]
        [InlineData("RANDOM")]
        [InlineData("Random")]
        public void CreateFromSpec_TypeIsCaseInsensitive(string type)
        {
            var spec = new AlgorithmSpec { Type = type, Parameters = AlgorithmFactoryTestData.Elem("""{ "Samples": 10 }""") };

            var (_, algo, _) = AlgorithmFactory.CreateFromSpec(spec);

            Assert.IsType<RandomSearchAlgorithm>(algo);
        }

        [Fact]
        public void CreateFromSpec_UnknownType_ThrowsArgumentException()
        {
            var spec = new AlgorithmSpec { Type = "DoesNotExist" };

            Assert.Throws<ArgumentException>(() => AlgorithmFactory.CreateFromSpec(spec));
        }
    }
}
