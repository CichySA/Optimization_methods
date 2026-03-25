using System;
using System.Linq;
using ExperimentRunner;
using PFSP.Algorithms.Evolutionary;
using PFSP.Solutions;
using PfspTests.ExperimentRunner.Factory;
using System.Text.Json;
using System.IO;
using Xunit;

namespace PfspTests.ExperimentRunner
{
    [Trait("Area", "ExperimentRunner")]
    [Trait("Component", "AlgorithmFactory")]
    [Trait("Kind", "Unit")]
    public class AlgorithmFactorySeedPropagationTests
    {
        [Fact]
        public void CreateFromSpec_WithSeedAndSamples_ThirdAlgorithmHasExpectedRecordedSeed()
        {
            var spec = new AlgorithmSpec
            {
                Type = "Evolutionary",
                Parameters = AlgorithmFactoryTestData.Elem("{ \"Seed\": 42, \"Samples\": 3 }")
            };

            var algos = AlgorithmFactory.CreateFromSpec(spec).ToList();
            Assert.True(algos.Count >= 3, "Expected at least three generated algorithm parameter sets");

            var third = algos[2];
            var algorithm = third.Algo;
            var parameters = Assert.IsType<EvolutionaryParameters>(third.Params);

            var instance = AlgorithmFactoryTestData.SmallInstance();
            var result = algorithm.Solve(instance, parameters);
            var resultParameters = Assert.IsType<EvolutionaryParameters>(result.Parameters);

            var record = ExperimentRunRecordFactory.Create(
                "test-instance",
                third.Name,
                resultParameters,
                resultParameters.Seed,
                result,
                DateTimeOffset.UtcNow);

            var expected = (1_000_003 * 2) ^ 42;
            Assert.Equal(expected, record.Seed);
            // Verify seed serializes correctly when the record is saved to JSON
            var fileName = Guid.NewGuid().ToString() + ".json";
            var outDir = Path.GetTempPath();
            global::ExperimentRunner.ResultSaver.SaveJson(outDir, fileName, record);
            var json = File.ReadAllText(Path.Combine(outDir, fileName));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var seedFromJson = root.GetProperty("Parameters").GetProperty("Seed").GetInt32();
            Assert.Equal(expected, seedFromJson);
            File.Delete(Path.Combine(outDir, fileName));
        }

        [Fact]
        public void CreateFromSpec_FromJsonElement_ThirdAlgorithmHasExpectedRecordedSeed()
        {
            var parametersElement = JsonDocument.Parse("{ \"Seed\": 42, \"Samples\": 3 }").RootElement.Clone();

            var spec = new AlgorithmSpec
            {
                Type = "Evolutionary",
                Parameters = parametersElement
            };

            var algos = AlgorithmFactory.CreateFromSpec(spec).ToList();
            Assert.True(algos.Count >= 3, "Expected at least three generated algorithm parameter sets");

            var third = algos[2];
            var algorithm = third.Algo;
            var parameters = Assert.IsType<EvolutionaryParameters>(third.Params);

            var instance = AlgorithmFactoryTestData.SmallInstance();
            var result = algorithm.Solve(instance, parameters);
            var resultParameters = Assert.IsType<EvolutionaryParameters>(result.Parameters);

            var record = ExperimentRunRecordFactory.Create(
                "test-instance",
                third.Name,
                resultParameters,
                resultParameters.Seed,
                result,
                DateTimeOffset.UtcNow);

            var expected = (1_000_003 * 2) ^ 42;
            Assert.Equal(expected, record.Seed);
            // Verify seed serializes correctly when the record is saved to JSON
            var fileName = Guid.NewGuid().ToString() + ".json";
            var outDir = Path.GetTempPath();
            global::ExperimentRunner.ResultSaver.SaveJson(outDir, fileName, record);
            var json = File.ReadAllText(Path.Combine(outDir, fileName));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var seedFromJson = root.GetProperty("Parameters").GetProperty("Seed").GetInt32();
            Assert.Equal(expected, seedFromJson);
            File.Delete(Path.Combine(outDir, fileName));
        }

        [Fact]
        public void CreateFromSpec_SeedFromAlgorithmParametersSection_ThirdAlgorithmHasExpectedRecordedSeed()
        {
            var algorithmSpecJson = JsonDocument.Parse("""
            {
              "Type": "Evolutionary",
              "Parameters": { "Seed": 42, "Samples": 3 }
            }
            """).RootElement.Clone();

            var spec = JsonSerializer.Deserialize<AlgorithmSpec>(algorithmSpecJson.GetRawText())!;

            var algos = AlgorithmFactory.CreateFromSpec(spec).ToList();
            Assert.True(algos.Count >= 3, "Expected at least three generated algorithm parameter sets");

            var third = algos[2];
            var algorithm = third.Algo;
            var parameters = Assert.IsType<EvolutionaryParameters>(third.Params);

            var instance = AlgorithmFactoryTestData.SmallInstance();
            var result = algorithm.Solve(instance, parameters);
            var resultParameters = Assert.IsType<EvolutionaryParameters>(result.Parameters);

            var record = ExperimentRunRecordFactory.Create(
                "test-instance",
                third.Name,
                resultParameters,
                resultParameters.Seed,
                result,
                DateTimeOffset.UtcNow);

            var expected = (1_000_003 * 2) ^ 42;
            Assert.Equal(expected, record.Seed);

            var fileName = Guid.NewGuid().ToString() + ".json";
            var outDir = Path.GetTempPath();
            global::ExperimentRunner.ResultSaver.SaveJson(outDir, fileName, record);
            var json = File.ReadAllText(Path.Combine(outDir, fileName));
            using var doc = JsonDocument.Parse(json);
            var seedFromJson = doc.RootElement.GetProperty("Parameters").GetProperty("Seed").GetInt32();
            Assert.Equal(expected, seedFromJson);
            File.Delete(Path.Combine(outDir, fileName));
        }
    }
}
