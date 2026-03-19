using System.Text.Json;
using PFSP.Algorithms;
using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.Greedy;
using PFSP.Algorithms.Random;

namespace ExperimentRunner
{
    public static class AlgorithmFactory
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public static (string Name, IAlgorithm Algo, IParameters Params) CreateFromSpec(AlgorithmSpec spec)
        {
            return spec.Type.ToLowerInvariant() switch
            {
                "random"        => CreateRandom(spec.Parameters),
                "evolutionary"  => CreateEvolutionary(spec.Parameters),
                "greedy"        => ("Greedy", new GreedyAlgorithm(), new GreedyParameters()),
                _               => throw new ArgumentException($"Unknown algorithm type '{spec.Type}'.")
            };
        }

        private static (string Name, IAlgorithm Algo, IParameters Params) CreateRandom(JsonElement parameters)
        {
            var dto = parameters.ValueKind != JsonValueKind.Undefined
                ? JsonSerializer.Deserialize<RandomParametersDto>(parameters, JsonOptions) ?? new RandomParametersDto()
                : new RandomParametersDto();

            var pars = dto.TimeLimitMs.HasValue
                ? RandomParameters.ForTimeLimit(TimeSpan.FromMilliseconds(dto.TimeLimitMs.Value), dto.Seed)
                : RandomParameters.ForRuns(dto.Samples, dto.Seed);

            return ($"Random_{pars.Samples}_s{pars.Seed}", new RandomAlgorithm(), pars);
        }

        private static (string Name, IAlgorithm Algo, IParameters Params) CreateEvolutionary(JsonElement parameters)
        {
            var raw = parameters.ValueKind != JsonValueKind.Undefined ? parameters.GetRawText() : "{}";
            var pars = EvolutionaryParameterFactory.FromJson(raw);
            EvolutionaryParameterFactory.Validate(pars);
            return ($"Evolutionary_p{pars.PopulationSize}_g{pars.Generations}_s{pars.Seed}", new EvolutionaryAlgorithm(), pars);
        }

        private sealed class RandomParametersDto
        {
            public int Seed { get; set; } = 0;
            public int Samples { get; set; } = 100;
            public int? TimeLimitMs { get; set; }
        }
    }
}
