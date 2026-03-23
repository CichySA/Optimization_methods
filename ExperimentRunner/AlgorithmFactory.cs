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

        public static IEnumerable<(string Name, IAlgorithm Algo, IParameters Params)> CreateFromSpec(AlgorithmSpec spec)
        {
            foreach (var parameterSet in ExpandParameterSets(spec.Parameters, spec.ParameterGrid2D))
            {
                foreach (var seededParameters in ExpandRuns(spec.Type, spec.Iterations, parameterSet))
                    yield return CreateSingleFromSpec(spec.Type, seededParameters);
            }
        }

        private static (string Name, IAlgorithm Algo, IParameters Params) CreateSingleFromSpec(string type, JsonElement parameters)
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

        private static IEnumerable<JsonElement> ExpandParameterSets(JsonElement parameters, JsonElement parameterGrid2D)
        {
            if (parameterGrid2D.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                yield return parameters;
                yield break;
            }

            var (firstName, firstValues, secondName, secondValues) = ParseParameterGrid2D(parameterGrid2D);
            foreach (var firstValue in firstValues)
            {
                foreach (var secondValue in secondValues)
                    yield return MergeParameters(parameters, (firstName, firstValue), (secondName, secondValue));
            }
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

            var pars = dto.TimeLimitMs.HasValue
                ? RandomSearchParameters.ForTimeLimit(TimeSpan.FromMilliseconds(dto.TimeLimitMs.Value), dto.Seed)
                : RandomSearchParameters.ForRuns(dto.Samples, dto.Seed);

            if (dto.Monitoring is not null)
                pars = pars with { Monitoring = dto.Monitoring };

            return ($"Random_{pars.Samples}_s{pars.Seed}", new RandomSearchAlgorithm(), pars);
        }

        private static (string Name, IAlgorithm Algo, IParameters Params) CreateEvolutionary(JsonElement parameters)
        {
            var pars = CreateEvolutionaryParameters(parameters);
            return ($"Evolutionary_p{pars.PopulationSize}_g{pars.Generations}_s{pars.Seed}", new EvolutionaryAlgorithm(), pars);
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

        private static (string FirstName, List<JsonElement> FirstValues, string SecondName, List<JsonElement> SecondValues)
            ParseParameterGrid2D(JsonElement grid)
        {
            if (grid.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("ParameterGrid2D must be a JSON object with exactly two parameter arrays.");

            var properties = grid.EnumerateObject().ToList();
            if (properties.Count != 2)
                throw new ArgumentException("ParameterGrid2D must contain exactly two named parameter arrays.");

            var first = properties[0];
            var second = properties[1];

            var firstValues = ParseGridValues(first.Name, first.Value);
            var secondValues = ParseGridValues(second.Name, second.Value);
            return (first.Name, firstValues, second.Name, secondValues);
        }

        private static List<JsonElement> ParseGridValues(string parameterName, JsonElement values)
        {
            if (values.ValueKind != JsonValueKind.Array)
                throw new ArgumentException($"ParameterGrid2D '{parameterName}' must be an array.");

            var parsed = values.EnumerateArray().Select(v => v.Clone()).ToList();
            if (parsed.Count == 0)
                throw new ArgumentException($"ParameterGrid2D '{parameterName}' array must not be empty.");

            return parsed;
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
            public AlgorithmMonitoringOptions? Monitoring { get; set; }
        }
    }
}
