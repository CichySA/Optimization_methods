using System.Text.Json;
using System.Text.Json.Nodes;
using PFSP.Algorithms;
using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.Greedy;
using PFSP.Algorithms.RandomSearch;
using PFSP.Algorithms.SimulatedAnnealing;

namespace ExperimentRunner
{
    public static class AlgorithmFactory
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public static IEnumerable<(string Name, IAlgorithm Algo, IParameters Params)> CreateManyFromSpec(AlgorithmSpec spec)
        {
            if (spec.ParameterGrid2D.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                yield return CreateFromSpec(spec);
                yield break;
            }

            var (firstName, firstValues, secondName, secondValues) = ParseParameterGrid2D(spec.ParameterGrid2D);

            foreach (var firstValue in firstValues)
            {
                foreach (var secondValue in secondValues)
                {
                    var mergedParameters = MergeParameters(
                        spec.Parameters,
                        firstName,
                        firstValue,
                        secondName,
                        secondValue);

                    var expandedSpec = new AlgorithmSpec
                    {
                        Type = spec.Type,
                        Parameters = mergedParameters
                    };
                    yield return CreateFromSpec(expandedSpec);
                }
            }
        }

        public static (string Name, IAlgorithm Algo, IParameters Params) CreateFromSpec(AlgorithmSpec spec)
        {
            return spec.Type.ToLowerInvariant() switch
            {
                "random"             => CreateRandom(spec.Parameters),
                "evolutionary"       => CreateEvolutionary(spec.Parameters),
                "simulatedannealing" => CreateSimulatedAnnealing(spec.Parameters),
                "greedy"             => ("Greedy", new GreedyAlgorithm(), new GreedyParameters()),
                "spt"                => ("SPT", new SptAlgorithm(), new GreedyParameters()),
                _                    => throw new ArgumentException($"Unknown algorithm type '{spec.Type}'.")
            };
        }

        private static (string Name, IAlgorithm Algo, IParameters Params) CreateRandom(JsonElement parameters)
        {
            var dto = parameters.ValueKind != JsonValueKind.Undefined
                ? JsonSerializer.Deserialize<RandomParametersDto>(parameters, JsonOptions) ?? new RandomParametersDto()
                : new RandomParametersDto();

            var pars = dto.TimeLimitMs.HasValue
                ? RandomSearchParameters.ForTimeLimit(TimeSpan.FromMilliseconds(dto.TimeLimitMs.Value), dto.Seed)
                : RandomSearchParameters.ForRuns(dto.Samples, dto.Seed);

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

        private static JsonElement MergeParameters(
            JsonElement baseParameters,
            string firstName,
            JsonElement firstValue,
            string secondName,
            JsonElement secondValue)
        {
            var merged = new JsonObject();

            if (baseParameters.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                // No base parameters; only grid parameters will be applied.
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

            merged[firstName] = JsonNode.Parse(firstValue.GetRawText());
            merged[secondName] = JsonNode.Parse(secondValue.GetRawText());

            using var doc = JsonDocument.Parse(merged.ToJsonString());
            return doc.RootElement.Clone();
        }

        private static string? TryGetParameterString(JsonElement parameters, string parameterName)
        {
            if (parameters.ValueKind != JsonValueKind.Object)
                return null;

            foreach (var property in parameters.EnumerateObject())
            {
                if (!string.Equals(property.Name, parameterName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (property.Value.ValueKind == JsonValueKind.String)
                    return property.Value.GetString();

                return property.Value.ToString();
            }

            return null;
        }

        private sealed class RandomParametersDto
        {
            public int Seed { get; set; } = 0;
            public int Samples { get; set; } = 100;
            public int? TimeLimitMs { get; set; }
        }
    }
}
