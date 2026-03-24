using ExperimentRunner;
using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.RandomSearch;
using PFSP.Algorithms.SimulatedAnnealing;

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

            var (_, _, pars) = Assert.Single(AlgorithmFactory.CreateFromSpec(spec));

            var rp = Assert.IsType<RandomSearchParameters>(pars);
            Assert.Equal(100, rp.Samples);
            Assert.Equal(0, rp.Seed);
        }

        [Fact]
        public void CreateFromSpec_EvolutionaryWithEmptyParameters_UsesDefaultPopulationAndGenerations()
        {
            var spec = new AlgorithmSpec { Type = "Evolutionary", Parameters = AlgorithmFactoryTestData.Elem("{}") };

            var (_, _, pars) = Assert.Single(AlgorithmFactory.CreateFromSpec(spec));

            var ep = Assert.IsType<EvolutionaryParameters>(pars);
            Assert.Equal(EvolutionaryParameters.DefaultPopulationSize, ep.PopulationSize);
            Assert.Equal(EvolutionaryParameters.DefaultGenerations, ep.Generations);
        }

        [Fact]
        public void CreateFromSpec_EvolutionaryWith2DGrid_ExpandsAllCombinations()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Evolutionary",
                Parameters = AlgorithmFactoryTestData.Elem("""
                {
                    "Seed": 9, "CrossoverRate": 0.7, "MutationRate": 0.1, "TournamentSize": 5,
                    "ParameterGrid": { "PopulationSize": [10, 20], "Generations": [3, 4] }
                }
                """)
            };

            var expanded = AlgorithmFactory.CreateFromSpec(spec).ToList();

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
        public void CreateFromSpec_RandomWith2DGrid_ExpandsAllCombinations()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Random",
                Parameters = AlgorithmFactoryTestData.Elem("""
                {
                    "ParameterGrid": { "Seed": [1, 2], "Samples": [10, 20] }
                }
                """)
            };

            var expanded = AlgorithmFactory.CreateFromSpec(spec).ToList();

            Assert.Equal(4, expanded.Count);

            var allPars = expanded.Select(e => Assert.IsType<RandomSearchParameters>(e.Params)).ToList();
            var combos = allPars.Select(p => (p.Seed, p.Samples)).ToHashSet();
            Assert.Contains((1, 10), combos);
            Assert.Contains((1, 20), combos);
            Assert.Contains((2, 10), combos);
            Assert.Contains((2, 20), combos);
        }

        [Fact]
        public void CreateFromSpec_RandomWithIterations_DerivesSeedsFromBaseSeed()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Random",
                Iterations = 3,
                Parameters = AlgorithmFactoryTestData.Elem("""{ "Seed": 7, "Samples": 10 }""")
            };

            var expanded = AlgorithmFactory.CreateFromSpec(spec).ToList();

            var allPars = expanded.Select(e => Assert.IsType<RandomSearchParameters>(e.Params)).ToList();
            Assert.Equal([7, unchecked(7 + 1_000_003), unchecked(7 + 2 * 1_000_003)], allPars.Select(p => p.Seed).ToArray());
        }

        [Fact]
        public void CreateFromSpec_SimulatedAnnealingWithIterations_DerivesSeedsFromBaseSeed()
        {
            var spec = new AlgorithmSpec
            {
                Type = "SimulatedAnnealing",
                Iterations = 2,
                Parameters = AlgorithmFactoryTestData.Elem("""{ "Seed": 11, "Iterations": 50 }""")
            };

            var expanded = AlgorithmFactory.CreateFromSpec(spec).ToList();

            var allPars = expanded.Select(e => Assert.IsType<SimulatedAnnealingParameters>(e.Params)).ToList();
            Assert.Equal([11, unchecked(11 + 1_000_003)], allPars.Select(p => p.Seed).ToArray());
        }

        [Fact]
        public void CreateFromSpec_GenericParameterNames_MapToCorrectEvolutionaryProperties()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Evolutionary",
                Parameters = AlgorithmFactoryTestData.Elem("""
                {
                    "Seed": 5, "CrossoverRate": 0.7, "MutationRate": 0.1, "TournamentSize": 5,
                    "ParameterGrid": { "Generations": [11], "PopulationSize": [222] }
                }
                """)
            };

            var expanded = AlgorithmFactory.CreateFromSpec(spec).ToList();

            var single = Assert.Single(expanded);
            var ep = Assert.IsType<EvolutionaryParameters>(single.Params);
            Assert.Equal(222, ep.PopulationSize);
            Assert.Equal(11, ep.Generations);
            Assert.Equal(5, ep.Seed);
        }

        // ── Parameter merging ───────────────────────────────────────────────

        [Fact]
        public void CreateFromSpec_GlobalParametersSupplyMissingValues()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Random",
                Parameters = AlgorithmFactoryTestData.Elem("""{ "Seed": 7 }""")
            };
            var global = AlgorithmFactoryTestData.Elem("""{ "Seed": 99, "Samples": 300 }""");

            var (_, _, pars) = Assert.Single(AlgorithmFactory.CreateFromSpec(spec, globalParameters: global));

            var rp = Assert.IsType<RandomSearchParameters>(pars);
            Assert.Equal(7, rp.Seed);      // local wins
            Assert.Equal(300, rp.Samples); // supplied by global
        }

        [Fact]
        public void CreateFromSpec_GlobalEvaluationBudgetAppliedToRandom()
        {
            var spec = new AlgorithmSpec { Type = "Random", Parameters = AlgorithmFactoryTestData.Elem("{}") };
            var global = AlgorithmFactoryTestData.Elem("""{"EvaluationBudget": 300}""");

            var (_, _, pars) = Assert.Single(AlgorithmFactory.CreateFromSpec(spec, globalParameters: global));

            Assert.Equal(300, Assert.IsType<RandomSearchParameters>(pars).Samples);
        }

        [Fact]
        public void CreateFromSpec_LocalEvaluationBudgetWinsOverGlobal()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Random",
                Parameters = AlgorithmFactoryTestData.Elem("""{"EvaluationBudget": 100}""")
            };
            var global = AlgorithmFactoryTestData.Elem("""{"EvaluationBudget": 999}""");

            var (_, _, pars) = Assert.Single(AlgorithmFactory.CreateFromSpec(spec, globalParameters: global));

            Assert.Equal(100, Assert.IsType<RandomSearchParameters>(pars).Samples);
        }

        // ── EA EvaluationBudget validation ──────────────────────────────────
        // NFE = PopulationSize + Generations * (PopulationSize - ElitismK)

        [Fact]
        public void CreateFromSpec_EvolutionaryNfeExceedsEvaluationBudget_ThrowsArgumentException()
        {
            // NFE = 10 + 10*(10-0) = 110 > 50
            var spec = new AlgorithmSpec
            {
                Type = "Evolutionary",
                Parameters = AlgorithmFactoryTestData.Elem(
                    """{ "PopulationSize": 10, "Generations": 10, "EvaluationBudget": 50 }""")
            };

            var ex = Assert.Throws<ArgumentException>(() => AlgorithmFactory.CreateFromSpec(spec).ToList());
            Assert.Contains("Computed NFE", ex.Message, StringComparison.Ordinal);
            Assert.Contains("EvaluationBudget", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void CreateFromSpec_EvolutionaryGlobalEvaluationBudgetViolatedByNfe_ThrowsArgumentException()
        {
            // NFE = 10 + 10*(10-0) = 110 > 50 (budget from global)
            var spec = new AlgorithmSpec
            {
                Type = "Evolutionary",
                Parameters = AlgorithmFactoryTestData.Elem("""{ "PopulationSize": 10, "Generations": 10 }""")
            };
            var global = AlgorithmFactoryTestData.Elem("""{"EvaluationBudget": 50}""");

            var ex = Assert.Throws<ArgumentException>(
                () => AlgorithmFactory.CreateFromSpec(spec, globalParameters: global).ToList());
            Assert.Contains("Computed NFE", ex.Message, StringComparison.Ordinal);
        }

        // ── SA EvaluationBudget validation ──────────────────────────────────
        // Error when Iterations > EvaluationBudget

        [Fact]
        public void CreateFromSpec_SimulatedAnnealingIterationsExceedEvaluationBudget_ThrowsArgumentException()
        {
            // 100 > 50
            var spec = new AlgorithmSpec
            {
                Type = "SimulatedAnnealing",
                Parameters = AlgorithmFactoryTestData.Elem("""{ "Iterations": 100, "EvaluationBudget": 50 }""")
            };

            var ex = Assert.Throws<ArgumentException>(() => AlgorithmFactory.CreateFromSpec(spec).ToList());
            Assert.Contains("Iterations", ex.Message, StringComparison.Ordinal);
            Assert.Contains("EvaluationBudget", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void CreateFromSpec_SimulatedAnnealingGlobalEvaluationBudgetViolatedByIterations_ThrowsArgumentException()
        {
            // 100 > 50 (budget from global)
            var spec = new AlgorithmSpec
            {
                Type = "SimulatedAnnealing",
                Parameters = AlgorithmFactoryTestData.Elem("""{ "Iterations": 100 }""")
            };
            var global = AlgorithmFactoryTestData.Elem("""{"EvaluationBudget": 50}""");

            var ex = Assert.Throws<ArgumentException>(
                () => AlgorithmFactory.CreateFromSpec(spec, globalParameters: global).ToList());
            Assert.Contains("Iterations", ex.Message, StringComparison.Ordinal);
            Assert.Contains("EvaluationBudget", ex.Message, StringComparison.Ordinal);
        }
    }
}
