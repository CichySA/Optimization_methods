using ExperimentRunner;
using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.RandomSearch;

namespace PfspTests.ExperimentRunner.Factory
{
    [Trait("Area", "ExperimentRunner")]
    [Trait("Component", "AlgorithmFactory")]
    [Trait("Kind", "Unit")]
    public class AlgorithmFactoryDefaultsAndGridTests
    {
        [Fact]
        public void CreateFromSpec_RandomWithEmptyParameters_UsesDefaultSamplesAndSeed()
        {
            var spec = new AlgorithmSpec { Type = "Random", Parameters = AlgorithmFactoryTestData.Elem("{}") };

            var (_, _, pars) = AlgorithmFactory.CreateFromSpec(spec);

            var rp = Assert.IsType<RandomSearchParameters>(pars);
            Assert.Equal(100, rp.Samples);
            Assert.Equal(0, rp.Seed);
        }

        [Fact]
        public void CreateFromSpec_EvolutionaryWithEmptyParameters_UsesDefaultPopulationAndGenerations()
        {
            var spec = new AlgorithmSpec { Type = "Evolutionary", Parameters = AlgorithmFactoryTestData.Elem("{}") };

            var (_, _, pars) = AlgorithmFactory.CreateFromSpec(spec);

            var ep = Assert.IsType<EvolutionaryParameters>(pars);
            Assert.Equal(EvolutionaryParameters.DefaultPopulationSize, ep.PopulationSize);
            Assert.Equal(EvolutionaryParameters.DefaultGenerations, ep.Generations);
        }

        [Fact]
        public void CreateManyFromSpec_EvolutionaryWith2DGrid_ExpandsAllCombinations()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Evolutionary",
                Parameters = AlgorithmFactoryTestData.Elem("""{ "Seed": 9, "CrossoverRate": 0.7, "MutationRate": 0.1, "TournamentSize": 5 }"""),
                ParameterGrid2D = AlgorithmFactoryTestData.Elem("""{ "PopulationSize": [10, 20], "Generations": [3, 4] }""")
            };

            var expanded = AlgorithmFactory.CreateManyFromSpec(spec).ToList();

            Assert.Equal(4, expanded.Count);

            var allPars = expanded.Select(e => Assert.IsType<EvolutionaryParameters>(e.Params)).ToList();
            Assert.All(allPars, p => Assert.Equal(9, p.Seed));

            var combos = allPars.Select(p => (p.PopulationSize, p.Generations)).ToHashSet();
            Assert.Contains((10, 3), combos);
            Assert.Contains((10, 4), combos);
            Assert.Contains((20, 3), combos);
            Assert.Contains((20, 4), combos);
        }

        [Fact]
        public void CreateManyFromSpec_RandomWith2DGrid_ExpandsAllCombinations()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Random",
                Parameters = AlgorithmFactoryTestData.Elem("{}"),
                ParameterGrid2D = AlgorithmFactoryTestData.Elem("""{ "Seed": [1, 2], "Samples": [10, 20] }""")
            };

            var expanded = AlgorithmFactory.CreateManyFromSpec(spec).ToList();

            Assert.Equal(4, expanded.Count);

            var allPars = expanded.Select(e => Assert.IsType<RandomSearchParameters>(e.Params)).ToList();
            var combos = allPars.Select(p => (p.Seed, p.Samples)).ToHashSet();
            Assert.Contains((1, 10), combos);
            Assert.Contains((1, 20), combos);
            Assert.Contains((2, 10), combos);
            Assert.Contains((2, 20), combos);
        }

        [Fact]
        public void CreateManyFromSpec_GenericParameterNames_MapToCorrectEvolutionaryProperties()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Evolutionary",
                Parameters = AlgorithmFactoryTestData.Elem("""{ "Seed": 5, "CrossoverRate": 0.7, "MutationRate": 0.1, "TournamentSize": 5 }"""),
                ParameterGrid2D = AlgorithmFactoryTestData.Elem("""{ "Generations": [11], "PopulationSize": [222] }""")
            };

            var expanded = AlgorithmFactory.CreateManyFromSpec(spec).ToList();

            var single = Assert.Single(expanded);
            var ep = Assert.IsType<EvolutionaryParameters>(single.Params);
            Assert.Equal(222, ep.PopulationSize);
            Assert.Equal(11, ep.Generations);
            Assert.Equal(5, ep.Seed);
        }
    }
}
