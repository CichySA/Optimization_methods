using System.Globalization;
using System.Text;
using System.Text.Json;
using PFSP.Algorithms.Evolutionary.Operators;
using PFSP.Algorithms.Evolutionary.Operators.CrossoverOperators;
using PFSP.Algorithms.Evolutionary.Operators.MutationOperators;
using PFSP.Algorithms.Evolutionary.Operators.SelectionOperators;
using PFSP.Algorithms.Monitoring;

namespace PFSP.Algorithms.Evolutionary
{
    public static class EvolutionaryParameterFactory
    {
        public const string SeedName = "Seed";
        public const string PopulationSizeName = "PopulationSize";
        public const string GenerationsName = "Generations";
        public const string CrossoverRateName = "CrossoverRate";
        public const string MutationRateName = "MutationRate";
        public const string TournamentSizeName = "TournamentSize";
        public const string SelectionMethodName = "SelectionMethod";
        public const string CrossoverMethodName = "CrossoverMethod";
        public const string MutationMethodName = "MutationMethod";

        public const string TournamentMethod = "Tournament";
        public const string OXMethod = "OX";
        public const string SwapMethod = "Swap";

        public static readonly Dictionary<string, Func<ISelectionMethod>> SelectionRegistry = new(StringComparer.OrdinalIgnoreCase)
        {
            { TournamentMethod, static () => new TournamentSelection() }
        };

        public static readonly Dictionary<string, Func<ICrossoverMethod>> CrossoverRegistry = new(StringComparer.OrdinalIgnoreCase)
        {
            { OXMethod, static () => new OrderCrossover() }
        };

        public static readonly Dictionary<string, Func<IMutationMethod>> MutationRegistry = new(StringComparer.OrdinalIgnoreCase)
        {
            { SwapMethod, static () => new SwapMutation() }
        };

        private static readonly Dictionary<string, string> ParameterFormatMapping = new()
        {
            { SeedName, "D" },
            { PopulationSizeName, "D" },
            { GenerationsName, "D" },
            { CrossoverRateName, "F2" },
            { MutationRateName, "F2" },
            { TournamentSizeName, "D" },
            { SelectionMethodName, "S" },
            { CrossoverMethodName, "S" },
            { MutationMethodName, "S" }
        };

        /// <summary>Creates parameters from a JSON string.</summary>
        public static EvolutionaryParameters FromJson(string json)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = JsonSerializer.Deserialize<EvolutionaryParametersDto>(json, options) ?? new EvolutionaryParametersDto();
            return FromDto(dto);
        }

        /// <summary>Serializes parameters to an indented JSON string.</summary>
        public static string ToJson(EvolutionaryParameters p)
        {
            return JsonSerializer.Serialize(ToDto(p), new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>Serializes parameters to a CSV string (header + values row).</summary>
        public static string ToCsv(EvolutionaryParameters p)
        {
            var dto = ToDto(p);
            var fields = BuildOutputFields(dto);
            var header = string.Join(",", fields.Select(f => f.Name));
            var values = string.Join(",", fields.Select(f => FormatValue(f.Name, f.Value, ParameterFormatMapping)));
            return header + Environment.NewLine + values;
        }

        /// <summary>Formats parameters as a human-readable multiline string.</summary>
        public static string ToConsoleString(EvolutionaryParameters p)
        {
            var dto = ToDto(p);

            var fields = BuildOutputFields(dto);
            var sb = new StringBuilder();
            for (int i = 0; i < fields.Length; i++)
            {
                var (name, value) = fields[i];
                sb.Append(name)
                  .Append(": ")
                  .Append(FormatValue(name, value, ParameterFormatMapping));

                if (i < fields.Length - 1)
                    sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Validates the parameters and throws <see cref="ArgumentException"/> listing all violations
        /// if any are found.
        /// </summary>
        public static void Validate(EvolutionaryParameters p)
        {
            List<string> errors = [];

            if (p.PopulationSize <= 0)
                errors.Add($"{PopulationSizeName} must be > 0 (was {p.PopulationSize}).");
            if (p.Generations <= 0)
                errors.Add($"{GenerationsName} must be > 0 (was {p.Generations}).");
            if (p.CrossoverRate is < 0.0 or > 1.0)
                errors.Add($"{CrossoverRateName} must be in [0, 1] (was {p.CrossoverRate}).");
            if (p.MutationRate is < 0.0 or > 1.0)
                errors.Add($"{MutationRateName} must be in [0, 1] (was {p.MutationRate}).");

            if (p.SelectionParameters is TournamentSelectionParameters tsp)
            {
                if (tsp.TournamentSize <= 0)
                    errors.Add($"{TournamentSizeName} must be > 0 (was {tsp.TournamentSize}).");
                if (tsp.TournamentSize > p.PopulationSize)
                    errors.Add($"{TournamentSizeName} ({tsp.TournamentSize}) must be <= {PopulationSizeName} ({p.PopulationSize}).");
            }

            if (p.SelectionMethod is null)
                errors.Add($"{SelectionMethodName} must not be null.");
            if (p.CrossoverMethod is null)
                errors.Add($"{CrossoverMethodName} must not be null.");
            if (p.MutationMethod is null)
                errors.Add($"{MutationMethodName} must not be null.");

            if (errors.Count > 0)
                throw new ArgumentException(
                    $"Invalid {nameof(EvolutionaryParameters)}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }

        // --- DTO (defaults mirror Default) ---
        private sealed class EvolutionaryParametersDto
        {
            public int Seed { get; set; } = EvolutionaryParameters.DefaultSeed;
            public int PopulationSize { get; set; } = EvolutionaryParameters.DefaultPopulationSize;
            public int Generations { get; set; } = EvolutionaryParameters.DefaultGenerations;
            public double CrossoverRate { get; set; } = EvolutionaryParameters.DefaultCrossoverRate;
            public double MutationRate { get; set; } = EvolutionaryParameters.DefaultMutationRate;
            public int TournamentSize { get; set; } = EvolutionaryParameters.DefaultTournamentSize;
            public string? SelectionMethod { get; set; } = TournamentMethod;
            public string? CrossoverMethod { get; set; } = OXMethod;
            public string? MutationMethod { get; set; } = SwapMethod;
            public AlgorithmMonitoringOptions Monitoring { get; set; } = new();
        }

        private static EvolutionaryParametersDto ToDto(EvolutionaryParameters p) => new()
        {
            Seed = p.Seed,
            PopulationSize = p.PopulationSize,
            Generations = p.Generations,
            CrossoverRate = p.CrossoverRate,
            MutationRate = p.MutationRate,
            TournamentSize = p.SelectionParameters is TournamentSelectionParameters tsp ? tsp.TournamentSize : p.TournamentSize,
            SelectionMethod = ResolveOperatorName(p.SelectionMethod, SelectionRegistry),
            CrossoverMethod = ResolveOperatorName(p.CrossoverMethod, CrossoverRegistry),
            MutationMethod = ResolveOperatorName(p.MutationMethod, MutationRegistry),
            Monitoring = p.Monitoring
        };

        private static EvolutionaryParameters FromDto(EvolutionaryParametersDto dto) => new()
        {
            Seed = dto.Seed,
            PopulationSize = dto.PopulationSize,
            Generations = dto.Generations,
            CrossoverRate = dto.CrossoverRate,
            MutationRate = dto.MutationRate,
            SelectionParameters = new TournamentSelectionParameters { TournamentSize = dto.TournamentSize },
            SelectionMethod = ResolveOperator(dto.SelectionMethod, SelectionRegistry, SelectionMethodName),
            CrossoverMethod = ResolveOperator(dto.CrossoverMethod, CrossoverRegistry, CrossoverMethodName),
            MutationMethod = ResolveOperator(dto.MutationMethod, MutationRegistry, MutationMethodName),
            Monitoring = dto.Monitoring
        };

        // Finds the registered name for an operator instance by matching its type.
        private static string ResolveOperatorName<T>(T method, Dictionary<string, Func<T>> registry) where T : class
        {
            var methodType = method.GetType();
            foreach (var item in registry)
                if (methodType == item.Value().GetType()) return item.Key;

            throw new ArgumentException($"Unknown operator instance of type {methodType.Name}. No matching type found in registry.");
        }

        // Finds a registered operator instance by name; throws if the name is missing or unrecognised.
        private static T ResolveOperator<T>(string? name, Dictionary<string, Func<T>> registry, string parameterName) where T : class
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    $"{parameterName} must be specified. Valid values: {string.Join(", ", registry.Keys)}");

            if (!registry.TryGetValue(name, out var found))
                throw new ArgumentException(
                    $"Unknown {parameterName} '{name}'. Valid values: {string.Join(", ", registry.Keys)}");

            return found();
        }

        private static (string Name, object? Value)[] BuildOutputFields(EvolutionaryParametersDto dto) =>
        [
            (SeedName, dto.Seed),
            (PopulationSizeName, dto.PopulationSize),
            (GenerationsName, dto.Generations),
            (CrossoverRateName, dto.CrossoverRate),
            (MutationRateName, dto.MutationRate),
            (TournamentSizeName, dto.TournamentSize),
            (SelectionMethodName, dto.SelectionMethod),
            (CrossoverMethodName, dto.CrossoverMethod),
            (MutationMethodName, dto.MutationMethod)
        ];

        private static string FormatValue(string parameterName, object? value, Dictionary<string, string> formats)
        {
            if (value is null) return string.Empty;
            if (!formats.TryGetValue(parameterName, out var format))
                return value.ToString() ?? string.Empty;

            if (format == "S")
                return value.ToString() ?? string.Empty;

            if (value is IFormattable formattable)
                return formattable.ToString(format, CultureInfo.InvariantCulture);

            return value.ToString() ?? string.Empty;
        }
    }
}
