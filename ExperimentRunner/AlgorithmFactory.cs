using System.Text.Json;
using System.Text.Json.Nodes;
using PFSP.Algorithms;
using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.Greedy;
using PFSP.Algorithms.Monitoring;
using PFSP.Algorithms.RandomSearch;
using PFSP.Algorithms.SimulatedAnnealing;

namespace ExperimentRunner
{
    public static class AlgorithmFactory
    {
        private const int SeedStride = 1_000_003;
        private const string SeedParameterName = "Seed";
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public static IEnumerable<(string Name, IAlgorithm Algo, IParameters Params)> CreateFromSpec(
            AlgorithmSpec spec,
            JsonElement globalParameters = default)
        {
            var mergedParameters = MergeParameters(globalParameters, spec.Parameters);
            foreach (var parameterSet in ExpandParameterSets(mergedParameters))
            {
                foreach (var seededParameters in ExpandRuns(spec.Type, spec.Iterations, parameterSet))
                    yield return CreateSingleFromSpec(spec.Type, seededParameters);
            }
        }

        private static (string Name, IAlgorithm Algo, IParameters Params) CreateSingleFromSpec(
            string type, JsonElement parameters)
        {
            return type.ToLowerInvariant() switch
            {
                "random"             => CreateRandom(parameters),
                "evolutionary"       => CreateEvolutionary(parameters),
                "simulatedannealing" => CreateSimulatedAnnealing(parameters),
                "greedy"             => ("Greedy", new GreedyAlgorithm(), new GreedyParameters()),
                "spt"                => ("SPT", new SptAlgorithm(), new GreedyParameters()),
                _                    => throw new ArgumentException($"Unknown algorithm type '{type}'.")
            };
        }

        // Expands the ParameterGrid key (if present) inside the merged parameters into all axis combinations.
        // ParameterGrid is removed from the yielded parameter sets so algorithms never see it.
        private static IEnumerable<JsonElement> ExpandParameterSets(JsonElement parameters)
        {
            if (!TryGetParameter(parameters, "ParameterGrid", out var grid)
                || grid.ValueKind is not JsonValueKind.Object)
            {
                yield return parameters;
                yield break;
            }

            var clean = RemoveParameter(parameters, "ParameterGrid");
            foreach (var combination in CartesianProduct(grid))
                yield return MergeParameters(clean, combination);
        }

        private static IEnumerable<JsonElement> ExpandRuns(string type, int iterations, JsonElement parameters)
        {
            if (!IsStochastic(type))
            {
                yield return parameters;
                yield break;
            }

            if (iterations <= 0)
                throw new ArgumentException($"Iterations must be greater than zero for stochastic algorithm '{type}'.", nameof(iterations));

            var baseSeed = TryGetParameterInt(parameters, SeedParameterName) ?? 0;
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                var seed = baseSeed == 0 ? 0 : unchecked(baseSeed + iteration * SeedStride);
                yield return MergeParameters(parameters, (SeedParameterName, JsonSerializer.SerializeToElement(seed)));
            }
        }

        private static (string Name, IAlgorithm Algo, IParameters Params) CreateRandom(JsonElement parameters)
        {
            var dto = parameters.ValueKind != JsonValueKind.Undefined
                ? JsonSerializer.Deserialize<RandomParametersDto>(parameters, JsonOptions) ?? new RandomParametersDto()
                : new RandomParametersDto();

            RandomSearchParameters pars;
            if (dto.TimeLimitMs.HasValue)
                pars = RandomSearchParameters.ForTimeLimit(TimeSpan.FromMilliseconds(dto.TimeLimitMs.Value), dto.Seed);
            else if (dto.EvaluationBudget.HasValue)
                pars = RandomSearchParameters.ForRuns((int)Math.Max(1L, dto.EvaluationBudget.Value), dto.Seed);
            else
                pars = RandomSearchParameters.ForRuns(dto.Samples, dto.Seed);

            if (dto.Monitoring is not null)
                pars = pars with { Monitoring = dto.Monitoring };

            return ($"Random_{pars.Samples}_s{pars.Seed}", new RandomSearchAlgorithm(), pars);
        }

        private static (string Name, IAlgorithm Algo, IParameters Params) CreateEvolutionary(JsonElement parameters)
        {
            var pars = CreateEvolutionaryParameters(parameters);
            var name = pars.ElitismK > 0
                ? $"Evolutionary_p{pars.PopulationSize}_g{pars.Generations}_k{pars.ElitismK}_s{pars.Seed}"
                : $"Evolutionary_p{pars.PopulationSize}_g{pars.Generations}_s{pars.Seed}";
            return (name, new EvolutionaryAlgorithm(), pars);
        }

        private static (string Name, IAlgorithm Algo, IParameters Params) CreateSimulatedAnnealing(JsonElement parameters)
        {
            var pars = CreateSimulatedAnnealingParameters(parameters);
            var neighborhood = TryGetParameterString(parameters, SimulatedAnnealingParameterFactory.NeighborhoodOperatorName)
                ?? SimulatedAnnealingParameterFactory.SwapNeighborhoodName;
            return ($"SimulatedAnnealing_n{neighborhood}_i{pars.Iterations}_s{pars.Seed}", new SimulatedAnnealingAlgorithm(), pars);
        }

        private static EvolutionaryParameters CreateEvolutionaryParameters(JsonElement parameters)
        {
            var raw = parameters.ValueKind != JsonValueKind.Undefined ? parameters.GetRawText() : "{}";
            var pars = EvolutionaryParameterFactory.FromJson(raw);
            EvolutionaryParameterFactory.Validate(pars);
            return pars;
        }

        private static SimulatedAnnealingParameters CreateSimulatedAnnealingParameters(JsonElement parameters)
        {
            var raw = parameters.ValueKind != JsonValueKind.Undefined ? parameters.GetRawText() : "{}";
            var pars = SimulatedAnnealingParameterFactory.FromJson(raw);
            SimulatedAnnealingParameterFactory.Validate(pars);
            return pars;
        }

        // Generates all axis combinations from a ParameterGrid JSON object.
        // Each axis is a named array; the result is the cartesian product across all axes.
        private static IEnumerable<(string Name, JsonElement Value)[]> CartesianProduct(JsonElement grid)
        {
            var axes = grid.EnumerateObject()
                .Select(p => (p.Name, Values: ParseGridValues(p.Name, p.Value)))
                .ToArray();

            if (axes.Length == 0) yield break;

            var sizes = Array.ConvertAll(axes, a => a.Values.Count);
            var indices = new int[axes.Length];
            do
            {
                var combo = new (string Name, JsonElement Value)[axes.Length];
                for (int i = 0; i < axes.Length; i++)
                    combo[i] = (axes[i].Name, axes[i].Values[indices[i]]);
                yield return combo;
            } while (Increment(indices, sizes));
        }

        private static bool Increment(int[] indices, int[] sizes)
        {
            for (int i = indices.Length - 1; i >= 0; i--)
            {
                if (++indices[i] < sizes[i]) return true;
                indices[i] = 0;
            }
            return false;
        }

        private static List<JsonElement> ParseGridValues(string parameterName, JsonElement values)
        {
            if (values.ValueKind != JsonValueKind.Array)
                throw new ArgumentException($"ParameterGrid '{parameterName}' must be an array.");

            var parsed = values.EnumerateArray().Select(v => v.Clone()).ToList();
            if (parsed.Count == 0)
                throw new ArgumentException($"ParameterGrid '{parameterName}' array must not be empty.");

            return parsed;
        }

        private static JsonElement RemoveParameter(JsonElement parameters, string key)
        {
            var obj = new JsonObject();
            foreach (var property in parameters.EnumerateObject())
                if (!string.Equals(property.Name, key, StringComparison.OrdinalIgnoreCase))
                    obj[property.Name] = JsonNode.Parse(property.Value.GetRawText());
            using var doc = JsonDocument.Parse(obj.ToJsonString());
            return doc.RootElement.Clone();
        }

        // Merges two JSON objects; overrideParameters wins for any shared keys.
        private static JsonElement MergeParameters(JsonElement baseParameters, JsonElement overrideParameters)
        {
            if (baseParameters.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
                return overrideParameters;
            if (overrideParameters.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
                return baseParameters;

            var overrides = overrideParameters.EnumerateObject()
                .Select(p => (p.Name, p.Value.Clone()))
                .ToArray();
            return MergeParameters(baseParameters, overrides);
        }

        private static JsonElement MergeParameters(JsonElement baseParameters, params (string Name, JsonElement Value)[] overrides)
        {
            var merged = new JsonObject();

            if (baseParameters.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
            }
            else if (baseParameters.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in baseParameters.EnumerateObject())
                    merged[property.Name] = JsonNode.Parse(property.Value.GetRawText());
            }
            else
            {
                throw new ArgumentException("Parameters must be a JSON object.");
            }

            foreach (var (name, value) in overrides)
                merged[name] = JsonNode.Parse(value.GetRawText());

            using var doc = JsonDocument.Parse(merged.ToJsonString());
            return doc.RootElement.Clone();
        }

        private static int? TryGetParameterInt(JsonElement parameters, string parameterName)
        {
            if (!TryGetParameter(parameters, parameterName, out var value))
                return null;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
                return number;

            return value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out number)
                ? number
                : null;
        }

        private static string? TryGetParameterString(JsonElement parameters, string parameterName)
        {
            if (!TryGetParameter(parameters, parameterName, out var value))
                return null;

            return value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : value.ToString();
        }

        private static bool TryGetParameter(JsonElement parameters, string parameterName, out JsonElement value)
        {
            if (parameters.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in parameters.EnumerateObject())
                {
                    if (string.Equals(property.Name, parameterName, StringComparison.OrdinalIgnoreCase))
                    {
                        value = property.Value;
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        private static bool IsStochastic(string type) => type.ToLowerInvariant() switch
        {
            "random" => true,
            "evolutionary" => true,
            "simulatedannealing" => true,
            _ => false
        };

        private sealed class RandomParametersDto
        {
            public int Seed { get; set; } = 0;
            public int Samples { get; set; } = 100;
            public int? TimeLimitMs { get; set; }
            public long? EvaluationBudget { get; set; }
            public AlgorithmMonitoringOptions? Monitoring { get; set; }
        }
    }
}
