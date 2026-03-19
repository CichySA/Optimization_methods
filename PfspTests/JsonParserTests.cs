using ExperimentRunner;
using System.Text.Json;
using PFSP.Algorithms.Evolutionary;

namespace PfspTests
{
    public class JsonParserTests
    {
        private static string WriteTemp(string json)
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, json);
            return path;
        }

        [Fact]
        public void Load_ValidJson_ParsesInstancesAlgorithmsAndOutDir()
        {
            var path = WriteTemp("""
            {
                "Instances": ["tai_20_5_0", "tai_100_10_0"],
                "OutDir": "my_results",
                "Algorithms": [
                    { "Type": "Random", "Parameters": { "Seed": 42, "Samples": 500 } },
                    { "Type": "Greedy",  "Parameters": {} }
                ]
            }
            """);

            var config = ExperimentRunnerConfigurationJsonParser.Load(path);

            // @TODO embedded algorithm parameters
            Assert.Equal(["tai_20_5_0", "tai_100_10_0"], config.Instances);
            Assert.Equal("my_results", config.OutDir);
            Assert.Equal(2, config.Algorithms.Length);
            Assert.Equal("Random", config.Algorithms[0].Type);
            Assert.Equal("Greedy",  config.Algorithms[1].Type);
        }

        [Fact]
        public void Load_MissingFile_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() =>
                ExperimentRunnerConfigurationJsonParser.Load("does_not_exist.json"));
        }

        [Fact]
        public void Load_EmptyJson_ReturnsDefaults()
        {
            var config = ExperimentRunnerConfigurationJsonParser.Load(WriteTemp("{}"));

            Assert.Equal(ExperimentRunnerConfiguration.DefaultInstances, config.Instances);
            Assert.Equal(ExperimentRunnerConfiguration.DefaultOutDir, config.OutDir);
            Assert.Equal(ExperimentRunnerConfiguration.DefaultAlgorithms.Length, config.Algorithms.Length);
        }

        [Fact]
        public void Load_PartialJson_OverridesOnlyPresentFields()
        {
            var config = ExperimentRunnerConfigurationJsonParser.Load(WriteTemp("""{ "OutDir": "custom_out" }"""));

            Assert.Equal("custom_out", config.OutDir);
            Assert.Equal(ExperimentRunnerConfiguration.DefaultInstances, config.Instances);
            Assert.Equal(ExperimentRunnerConfiguration.DefaultAlgorithms.Length, config.Algorithms.Length);
        }

        [Fact]
        public void Load_AlgorithmParameters_AreAccessibleAsJsonElement()
        {
            var path = WriteTemp("""
            {
                "Algorithms": [
                    { "Type": "Random", "Parameters": { "Seed": 99, "Samples": 777 } }
                ]
            }
            """);

            var spec = ExperimentRunnerConfigurationJsonParser.Load(path).Algorithms.Single();

            Assert.Equal("Random", spec.Type);
            Assert.Equal(System.Text.Json.JsonValueKind.Object, spec.Parameters.ValueKind);
            Assert.Equal(99,  spec.Parameters.GetProperty("Seed").GetInt32());
            Assert.Equal(777, spec.Parameters.GetProperty("Samples").GetInt32());
        }

        [Fact]
        public void Load_IsCaseInsensitiveForPropertyNames()
        {
            var config = ExperimentRunnerConfigurationJsonParser.Load(WriteTemp("""
            {
                "outdir": "insensitive",
                "instances": ["tai_20_5_0"]
            }
            """));

            Assert.Equal("insensitive", config.OutDir);
            Assert.Single(config.Instances);
        }

        [Fact]
        public void Load_EvolutionaryAlgorithm_ParsesAllParameters()
        {
            var path = WriteTemp("""
            {
                "Algorithms": [
                    {
                        "Type": "Evolutionary",
                        "Parameters": {
                            "Seed": 5,
                            "PopulationSize": 200,
                            "Generations": 300,
                            "CrossoverRate": 0.85,
                            "MutationRate": 0.05,
                            "TournamentSize": 7,
                            "SelectionMethod": "Tournament",
                            "CrossoverMethod": "OX",
                            "MutationMethod": "Swap"
                        }
                    }
                ]
            }
            """);

            var spec = ExperimentRunnerConfigurationJsonParser.Load(path).Algorithms.Single();
            var p    = spec.Parameters;

            Assert.Equal("Evolutionary", spec.Type);
            Assert.Equal(JsonValueKind.Object, p.ValueKind);
            Assert.Equal(5,    p.GetProperty("Seed").GetInt32());
            Assert.Equal(200,  p.GetProperty("PopulationSize").GetInt32());
            Assert.Equal(300,  p.GetProperty("Generations").GetInt32());
            Assert.Equal(0.85, p.GetProperty("CrossoverRate").GetDouble(), precision: 10);
            Assert.Equal(0.05, p.GetProperty("MutationRate").GetDouble(),  precision: 10);
            Assert.Equal(7,    p.GetProperty("TournamentSize").GetInt32());
            Assert.Equal("Tournament", p.GetProperty("SelectionMethod").GetString());
            Assert.Equal("OX",         p.GetProperty("CrossoverMethod").GetString());
            Assert.Equal("Swap",       p.GetProperty("MutationMethod").GetString());
        }

        [Fact]
        public void Load_EvolutionaryAlgorithm_WithDefaults_ParsesCorrectly()
        {
            var path = WriteTemp("""
            {
                "Algorithms": [ { "Type": "Evolutionary", "Parameters": {} } ]
            }
            """);

            var spec = ExperimentRunnerConfigurationJsonParser.Load(path).Algorithms.Single();

            Assert.Equal("Evolutionary", spec.Type);
            Assert.Equal(JsonValueKind.Object, spec.Parameters.ValueKind);
        }

        [Fact]
        public void Load_MixedAlgorithms_ParsesRandomAndEvolutionaryTogether()
        {
            var path = WriteTemp("""
            {
                "Instances": ["tai_20_5_0"],
                "OutDir": "out",
                "Algorithms": [
                    { "Type": "Random",      "Parameters": { "Seed": 1, "Samples": 50 } },
                    { "Type": "Evolutionary","Parameters": { "PopulationSize": 40, "Generations": 20 } },
                    { "Type": "Greedy",      "Parameters": {} }
                ]
            }
            """);

            var config = ExperimentRunnerConfigurationJsonParser.Load(path);

            Assert.Equal(3, config.Algorithms.Length);
            Assert.Equal("Random",      config.Algorithms[0].Type);
            Assert.Equal("Evolutionary",config.Algorithms[1].Type);
            Assert.Equal("Greedy",      config.Algorithms[2].Type);

            var evoPars = config.Algorithms[1].Parameters;
            Assert.Equal(40, evoPars.GetProperty("PopulationSize").GetInt32());
            Assert.Equal(20, evoPars.GetProperty("Generations").GetInt32());
        }

        [Fact]
        public void Load_EvolutionaryAlgorithm_ParametersRoundTripViaFactory()
        {
            var path = WriteTemp("""
            {
                "Algorithms": [
                    {
                        "Type": "Evolutionary",
                        "Parameters": {
                            "Seed": 42,
                            "PopulationSize": 150,
                            "Generations": 75,
                            "CrossoverRate": 0.6,
                            "MutationRate": 0.2,
                            "TournamentSize": 3
                        }
                    }
                ]
            }
            """);

            var spec = ExperimentRunnerConfigurationJsonParser.Load(path).Algorithms.Single();
            var (_, _, pars) = AlgorithmFactory.CreateFromSpec(spec);
            var ep = Assert.IsType<EvolutionaryParameters>(pars);

            Assert.Equal(42,  ep.Seed);
            Assert.Equal(150, ep.PopulationSize);
            Assert.Equal(75,  ep.Generations);
            Assert.Equal(0.6, ep.CrossoverRate,  precision: 10);
            Assert.Equal(0.2, ep.MutationRate,   precision: 10);
            Assert.Equal(3,   ep.TournamentSize);
        }
    }
}
