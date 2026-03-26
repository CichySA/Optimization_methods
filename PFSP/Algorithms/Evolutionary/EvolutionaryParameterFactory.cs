using System.Globalization;
using System.Text.Json;
using PFSP.Algorithms.Evolutionary.Operators;
using PFSP.Algorithms.Evolutionary.Operators.CrossoverOperators;
using PFSP.Algorithms.Evolutionary.Operators.MutationOperators;
using PFSP.Algorithms.Evolutionary.Operators.SelectionOperators;
using PFSP.Monitoring;

namespace PFSP.Algorithms.Evolutionary
{
    public static class EvolutionaryParameterFactory
    {
        public const string SeedName = "Seed";
        public const string PopulationSizeName = "PopulationSize";
        public const string GenerationsName = "Generations";
        public const string ElitismKName = "ElitismK";
        public const string EvaluationBudgetName = "EvaluationBudget";
        public const string CrossoverRateName = "CrossoverRate";
        public const string MutationRateName = "MutationRate";
        public const string TournamentSizeName = "TournamentSize";
        public const string SelectionMethodName = "SelectionMethod";
        public const string CrossoverMethodName = "CrossoverMethod";
        public const string MutationMethodName = "MutationMethod";

        public static readonly Dictionary<string, Func<ISelectionMethod>> SelectionRegistry = new(StringComparer.OrdinalIgnoreCase)
        {
            { TournamentSelection.Name, static () => new TournamentSelection() }
        };

        public static readonly Dictionary<string, Func<ICrossoverMethod>> CrossoverRegistry = new(StringComparer.OrdinalIgnoreCase)
        {
            { OrderCrossover.Name, static () => new OrderCrossover() },
            { CycleCrossover.Name, static () => new CycleCrossover() }
        };

        public static readonly Dictionary<string, Func<IMutationMethod>> MutationRegistry = new(StringComparer.OrdinalIgnoreCase)
        {
            { SwapMutation.Name, static () => new SwapMutation() },
            { InsertMutation.Name, static () => new InsertMutation() }
        };

        private static readonly Dictionary<string, string> ParameterFormatMapping = new()
        {
            { SeedName, "D" },
            { PopulationSizeName, "D" },
            { GenerationsName, "D" },
            { ElitismKName, "D" },
            { EvaluationBudgetName, "D" },
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

        /// <summary>Constructs the canonical algorithm name from parameters using the factory's field definitions and format mapping.</summary>
        public static string ToName(EvolutionaryParameters p)
        {
            var dto = ToDto(p);
            var fields = BuildOutputFields(dto);
            return "Evolutionary_" + string.Join("_", fields.Select(f => $"{f.Name}{FormatValue(f.Name, f.Value, ParameterFormatMapping)}"));
        }

        /// <summary>
        /// Validates the parameters and throws <see cref="ArgumentException"/> listing all violations
        /// if any are found. If EvaluationBudget &gt; 0, PopulationSize/Generations are considered
        /// budget-driven and both should not be explicitly provided at the same time.
        /// </summary>
        public static void Validate(EvolutionaryParameters p)
        {
            var errors = new List<string>();

            if (p.PopulationSize <= 0)
                errors.Add($"{PopulationSizeName} must be > 0 (was {p.PopulationSize}).");
            if (p.Generations <= 0)
                errors.Add($"{GenerationsName} must be > 0 (was {p.Generations}).");
            if (p.ElitismK < 0)
                errors.Add($"{ElitismKName} must be >= 0 (was {p.ElitismK}).");
            if (p.ElitismK >= p.PopulationSize)
                errors.Add($"{ElitismKName} ({p.ElitismK}) must be < {PopulationSizeName} ({p.PopulationSize}).");
            if (p.EvaluationBudget < 0)
                errors.Add($"{EvaluationBudgetName} must be >= 0 (was {p.EvaluationBudget}).");


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
                throw new ArgumentException($"Invalid {nameof(EvolutionaryParameters)}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }



        // --- DTO (defaults mirror Default) ---
        private sealed class EvolutionaryParametersDto
        {
            public int Seed { get; set; } = EvolutionaryParameters.DefaultSeed;
            public int PopulationSize { get; set; } = EvolutionaryParameters.DefaultPopulationSize;
            public int Generations { get; set; } = EvolutionaryParameters.DefaultGenerations;
            public int ElitismK { get; set; } = EvolutionaryParameters.DefaultElitismK;
            public long EvaluationBudget { get; set; } = EvolutionaryParameters.DefaultEvaluationBudget;
            public double CrossoverRate { get; set; } = EvolutionaryParameters.DefaultCrossoverRate;
            public double MutationRate { get; set; } = EvolutionaryParameters.DefaultMutationRate;
            public int TournamentSize { get; set; } = EvolutionaryParameters.DefaultTournamentSize;
            public string? SelectionMethod { get; set; } = TournamentSelection.Name;
            public string? CrossoverMethod { get; set; } = OrderCrossover.Name;
            public string? MutationMethod { get; set; } = SwapMutation.Name;
            public AlgorithmMonitoringOptions Monitoring { get; set; } = new();
        }

        private static EvolutionaryParametersDto ToDto(EvolutionaryParameters p) => new()
        {
            Seed = p.Seed,
            PopulationSize = p.PopulationSize,
            Generations = p.Generations,
            ElitismK = p.ElitismK,
            EvaluationBudget = p.EvaluationBudget,
            CrossoverRate = p.CrossoverRate,
            MutationRate = p.MutationRate,
            TournamentSize = p.SelectionParameters is TournamentSelectionParameters tsp ? tsp.TournamentSize : p.TournamentSize,
            SelectionMethod = p.SelectionMethod.Name,
            CrossoverMethod = p.CrossoverMethod.Name,
            MutationMethod = p.MutationMethod.Name,
            Monitoring = p.Monitoring
        };

        /// <summary>
        /// If ElitismK and EvaluationBudget are set, they change the effective number of generations that can be run within the evaluation budget.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private static EvolutionaryParameters FromDto(EvolutionaryParametersDto dto)
        {
            int generations = dto.Generations;
            if (dto.ElitismK > 0 && dto.EvaluationBudget > 0 && dto.PopulationSize > dto.ElitismK)
            {
                long remainder = dto.EvaluationBudget - dto.PopulationSize;
                if (remainder > 0)
                    generations = (int)(1 + remainder / (dto.PopulationSize - dto.ElitismK));
            }

            return new()
            {
                Seed = dto.Seed,
                PopulationSize = dto.PopulationSize,
                Generations = generations,
                ElitismK = dto.ElitismK,
                EvaluationBudget = dto.EvaluationBudget,
                CrossoverRate = dto.CrossoverRate,
                MutationRate = dto.MutationRate,
                SelectionParameters = new TournamentSelectionParameters { TournamentSize = dto.TournamentSize },
                SelectionMethod = ResolveOperator(dto.SelectionMethod, SelectionRegistry, SelectionMethodName),
                CrossoverMethod = ResolveOperator(dto.CrossoverMethod, CrossoverRegistry, CrossoverMethodName),
                MutationMethod = ResolveOperator(dto.MutationMethod, MutationRegistry, MutationMethodName),
                Monitoring = dto.Monitoring
            };
        }

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
            (ElitismKName, dto.ElitismK),
            (EvaluationBudgetName, dto.EvaluationBudget),
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
