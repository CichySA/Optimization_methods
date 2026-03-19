using System.Text.Json;
using ExperimentRunner;
using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.Greedy;
using PFSP.Algorithms.Random;
using PFSP.Evaluators;
using PFSP.Instances;

namespace PfspTests
{
    public class AlgorithmFactoryTests
    {
        // Creates a cloned JsonElement that outlives its JsonDocument.
        private static JsonElement Elem(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();

        private static Instance SmallInstance() =>
            Instance.Create(
                new double[,]
                {
                    { 30, 29, 19, 50 },
                    { 39, 69,  6, 73 },
                    {  3, 86, 58, 60 },
                    { 72, 74, 51, 15 },
                    { 24, 47, 22, 90 }
                },
                new TotalFlowTimeEvaluator());

        // --- type dispatch ---

        [Fact]
        public void CreateFromSpec_RandomType_ReturnsRandomAlgorithmWithCorrectParameters()
        {
            var spec = new AlgorithmSpec { Type = "Random", Parameters = Elem("""{ "Seed": 7, "Samples": 50 }""") };

            var (name, algo, pars) = AlgorithmFactory.CreateFromSpec(spec);

            Assert.Contains("Random", name);
            Assert.IsType<RandomAlgorithm>(algo);
            var rp = Assert.IsType<RandomParameters>(pars);
            Assert.Equal(7,  rp.Seed);
            Assert.Equal(50, rp.Samples);
        }

        [Fact]
        public void CreateFromSpec_EvolutionaryType_ReturnsEvolutionaryAlgorithmWithCorrectParameters()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Evolutionary",
                Parameters = Elem("""{ "PopulationSize": 30, "Generations": 5, "Seed": 3 }""")
            };

            var (name, algo, pars) = AlgorithmFactory.CreateFromSpec(spec);

            Assert.Contains("Evolutionary", name);
            Assert.IsType<EvolutionaryAlgorithm>(algo);
            var ep = Assert.IsType<EvolutionaryParameters>(pars);
            Assert.Equal(30, ep.PopulationSize);
            Assert.Equal(5,  ep.Generations);
            Assert.Equal(3,  ep.Seed);
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

        [Theory]
        [InlineData("random")]
        [InlineData("RANDOM")]
        [InlineData("Random")]
        public void CreateFromSpec_TypeIsCaseInsensitive(string type)
        {
            var spec = new AlgorithmSpec { Type = type, Parameters = Elem("""{ "Samples": 10 }""") };

            var (_, algo, _) = AlgorithmFactory.CreateFromSpec(spec);

            Assert.IsType<RandomAlgorithm>(algo);
        }

        [Fact]
        public void CreateFromSpec_UnknownType_ThrowsArgumentException()
        {
            var spec = new AlgorithmSpec { Type = "DoesNotExist" };

            Assert.Throws<ArgumentException>(() => AlgorithmFactory.CreateFromSpec(spec));
        }

        // --- algorithm name format ---

        [Fact]
        public void CreateFromSpec_RandomName_ContainsSamplesAndSeed()
        {
            var spec = new AlgorithmSpec { Type = "Random", Parameters = Elem("""{ "Seed": 42, "Samples": 200 }""") };

            var (name, _, _) = AlgorithmFactory.CreateFromSpec(spec);

            Assert.Contains("200", name);
            Assert.Contains("42",  name);
        }

        [Fact]
        public void CreateFromSpec_EvolutionaryName_ContainsPopulationGenerationsAndSeed()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Evolutionary",
                Parameters = Elem("""{ "PopulationSize": 50, "Generations": 10, "Seed": 1 }""")
            };

            var (name, _, _) = AlgorithmFactory.CreateFromSpec(spec);

            Assert.Contains("50", name);
            Assert.Contains("10", name);
            Assert.Contains("1",  name);
        }

        // --- default parameter fallback ---

        [Fact]
        public void CreateFromSpec_RandomWithEmptyParameters_UsesDefaultSamplesAndSeed()
        {
            var spec = new AlgorithmSpec { Type = "Random", Parameters = Elem("{}") };

            var (_, _, pars) = AlgorithmFactory.CreateFromSpec(spec);

            var rp = Assert.IsType<RandomParameters>(pars);
            Assert.Equal(100, rp.Samples);
            Assert.Equal(0,   rp.Seed);
        }

        [Fact]
        public void CreateFromSpec_EvolutionaryWithEmptyParameters_UsesDefaultPopulationAndGenerations()
        {
            var spec = new AlgorithmSpec { Type = "Evolutionary", Parameters = Elem("{}") };

            var (_, _, pars) = AlgorithmFactory.CreateFromSpec(spec);

            var ep = Assert.IsType<EvolutionaryParameters>(pars);
            Assert.Equal(EvolutionaryParameters.DefaultPopulationSize, ep.PopulationSize);
            Assert.Equal(EvolutionaryParameters.DefaultGenerations,    ep.Generations);
        }

        // --- integration: algorithms actually run on a real instance ---

        [Fact]
        public void CreateFromSpec_Random_SolvesInstance_ReturnsBestSolution()
        {
            var spec = new AlgorithmSpec { Type = "Random", Parameters = Elem("""{ "Seed": 1, "Samples": 20 }""") };
            var (_, algo, pars) = AlgorithmFactory.CreateFromSpec(spec);

            var result = algo.Solve(SmallInstance(), pars, TestContext.Current.CancellationToken);

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
                Parameters = Elem("""{ "Seed": 1, "PopulationSize": 10, "Generations": 5 }""")
            };
            var (_, algo, pars) = AlgorithmFactory.CreateFromSpec(spec);

            var result = algo.Solve(SmallInstance(), pars, TestContext.Current.CancellationToken);

            Assert.NotNull(result.Best);
            Assert.True(result.Best.Cost > 0);
        }

        [Fact]
        public void CreateFromSpec_Greedy_SolvesInstance_ReturnsBestSolution()
        {
            var spec = new AlgorithmSpec { Type = "Greedy" };
            var (_, algo, pars) = AlgorithmFactory.CreateFromSpec(spec);

            var result = algo.Solve(SmallInstance(), pars, TestContext.Current.CancellationToken);

            Assert.NotNull(result.Best);
            Assert.True(result.Best.Cost > 0);
        }
    }
}
