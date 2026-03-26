using System.Globalization;
using System.Text;
using System.Text.Json;
using PFSP.Algorithms.SimulatedAnnealing.Operators;
using PFSP.Algorithms.SimulatedAnnealing.Operators.AcceptanceFunctions;
using PFSP.Algorithms.SimulatedAnnealing.Operators.CoolingSchedules;
using PFSP.Algorithms.SimulatedAnnealing.Operators.NeighborhoodOperators;
using PFSP.Monitoring;

namespace PFSP.Algorithms.SimulatedAnnealing
{
    public static class SimulatedAnnealingParameterFactory
    {
        public const string SeedName = "Seed";
        public const string IterationsName = "Iterations";
        public const string EvaluationBudgetName = "EvaluationBudget";
        public const string InitialTemperatureName = "InitialTemperature";
        public const string CoolingRateName = "CoolingRate";
        public const string MinimumTemperatureName = "MinimumTemperature";
        public const string NeighborhoodOperatorName = "NeighborhoodOperator";
        public const string AcceptanceFunctionName = "AcceptanceFunction";
        public const string CoolingScheduleName = "CoolingSchedule";

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public static readonly Dictionary<string, Func<INeighborhoodOperator>> NeighborhoodRegistry = new(StringComparer.OrdinalIgnoreCase)
        {
            { SwapNeighborhood.Name, static () => new SwapNeighborhood() },
            { InsertNeighborhood.Name, static () => new InsertNeighborhood() },
            { ReverseNeighborhood.Name, static () => new ReverseNeighborhood() }
        };

        public static readonly Dictionary<string, Func<IAcceptanceFunction>> AcceptanceRegistry = new(StringComparer.OrdinalIgnoreCase)
        {
            { ProbabilisticAcceptanceFunction.Name, static () => new ProbabilisticAcceptanceFunction() },
            { ThresholdAcceptanceFunction.Name, static () => new ThresholdAcceptanceFunction() }
        };

        public static readonly Dictionary<string, Func<ICoolingSchedule>> CoolingRegistry = new(StringComparer.OrdinalIgnoreCase)
        {
            { ExponentialCoolingSchedule.Name, static () => new ExponentialCoolingSchedule() },
            { LinearCoolingSchedule.Name, static () => new LinearCoolingSchedule() }
        };

        private static readonly Dictionary<string, string> ParameterFormatMapping = new()
        {
            { SeedName, "D" },
            { IterationsName, "D" },
            { EvaluationBudgetName, "D" },
            { InitialTemperatureName, "F2" },
            { CoolingRateName, "F4" },
            { MinimumTemperatureName, "F4" },
            { NeighborhoodOperatorName, "S" },
            { AcceptanceFunctionName, "S" },
            { CoolingScheduleName, "S" }
        };

        /// <summary>Constructs the canonical algorithm name from parameters using the factory's field definitions and format mapping.</summary>
        public static string ToName(SimulatedAnnealingParameters p)
        {
            var dto = ToDto(p);
            var fields = BuildOutputFields(dto);
            return "SimulatedAnnealing_" + string.Join("_", fields.Select(f => $"{f.Name}{FormatValue(f.Name, f.Value, ParameterFormatMapping)}"));
        }

        public static SimulatedAnnealingParameters Default => SimulatedAnnealingParameters.Default;

        public static SimulatedAnnealingParameters FromJson(string json)
        {
            var dto = JsonSerializer.Deserialize<SimulatedAnnealingParametersDto>(json, JsonOptions) ?? new SimulatedAnnealingParametersDto();
            return FromDto(dto);
        }

        public static string ToJson(SimulatedAnnealingParameters p)
        {
            return JsonSerializer.Serialize(ToDto(p), new JsonSerializerOptions { WriteIndented = true });
        }

        public static string ToCsv(SimulatedAnnealingParameters p)
        {
            var dto = ToDto(p);
            var fields = BuildOutputFields(dto);
            var header = string.Join(",", fields.Select(f => f.Name));
            var values = string.Join(",", fields.Select(f => FormatValue(f.Name, f.Value, ParameterFormatMapping)));
            return header + Environment.NewLine + values;
        }

        public static string ToConsoleString(SimulatedAnnealingParameters p)
        {
            var dto = ToDto(p);
            var fields = BuildOutputFields(dto);
            var sb = new StringBuilder();
            for (int i = 0; i < fields.Length; i++)
            {
                var (name, value) = fields[i];
                sb.Append(name).Append(": ").Append(FormatValue(name, value, ParameterFormatMapping));
                if (i < fields.Length - 1) sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        public static void Validate(SimulatedAnnealingParameters p)
        {
            List<string> errors = [];

            if (p.Iterations <= 0)
                errors.Add($"{IterationsName} must be > 0 (was {p.Iterations}).");
            if (p.EvaluationBudget < 0)
                errors.Add($"{EvaluationBudgetName} must be >= 0 (was {p.EvaluationBudget}).");
            if (p.EvaluationBudget > 0 && p.Iterations > p.EvaluationBudget)
                errors.Add($"{IterationsName} ({p.Iterations}) exceeds {EvaluationBudgetName} ({p.EvaluationBudget}). Reduce {IterationsName} or increase {EvaluationBudgetName}.");
            if (p.InitialTemperature <= 0)
                errors.Add($"{InitialTemperatureName} must be > 0 (was {p.InitialTemperature}).");
            if (p.CoolingRate <= 0 || p.CoolingRate >= 1)
                errors.Add($"{CoolingRateName} must be in (0, 1) (was {p.CoolingRate}).");
            if (p.MinimumTemperature < 0)
                errors.Add($"{MinimumTemperatureName} must be >= 0 (was {p.MinimumTemperature}).");
            if (p.NeighborhoodOperator is null)
                errors.Add($"{NeighborhoodOperatorName} must not be null.");
            if (p.AcceptanceFunction is null)
                errors.Add($"{AcceptanceFunctionName} must not be null.");
            if (p.CoolingSchedule is null)
                errors.Add($"{CoolingScheduleName} must not be null.");

            var coolingName = p.CoolingSchedule.Name;
            if (!IsCoolingParametersCompatible(coolingName, p.CoolingScheduleParameters))
                errors.Add($"CoolingScheduleParameters type '{p.CoolingScheduleParameters.GetType().Name}' is not compatible with CoolingSchedule '{coolingName}'.");

            if (errors.Count > 0)
                throw new ArgumentException($"Invalid {nameof(SimulatedAnnealingParameters)}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }

        private sealed class SimulatedAnnealingParametersDto
        {
            public int Seed { get; set; } = SimulatedAnnealingParameters.DefaultSeed;
            public int Iterations { get; set; } = SimulatedAnnealingParameters.DefaultIterations;
            public long EvaluationBudget { get; set; } = SimulatedAnnealingParameters.DefaultEvaluationBudget;
            public double InitialTemperature { get; set; } = SimulatedAnnealingParameters.DefaultInitialTemperature;
            public double CoolingRate { get; set; } = SimulatedAnnealingParameters.DefaultCoolingRate;
            public double MinimumTemperature { get; set; } = SimulatedAnnealingParameters.DefaultMinimumTemperature;
            public string? NeighborhoodOperator { get; set; } = SwapNeighborhood.Name;
            public string? AcceptanceFunction { get; set; } = ProbabilisticAcceptanceFunction.Name;
            public string? CoolingSchedule { get; set; } = ExponentialCoolingSchedule.Name;
            public JsonElement CoolingScheduleParameters { get; set; }
            public AlgorithmMonitoringOptions Monitoring { get; set; } = new();
        }

        private static SimulatedAnnealingParametersDto ToDto(SimulatedAnnealingParameters p)
        {
            var coolingName = p.CoolingSchedule.Name;
            return new SimulatedAnnealingParametersDto
            {
                Seed = p.Seed,
                Iterations = p.Iterations,
                EvaluationBudget = p.EvaluationBudget,
                InitialTemperature = p.InitialTemperature,
                CoolingRate = p.CoolingRate,
                MinimumTemperature = p.MinimumTemperature,
                NeighborhoodOperator = p.NeighborhoodOperator.Name,
                AcceptanceFunction = p.AcceptanceFunction.Name,
                CoolingSchedule = coolingName,
                CoolingScheduleParameters = SerializeCoolingScheduleParameters(coolingName, p.CoolingScheduleParameters),
                Monitoring = p.Monitoring
            };
        }

        // FromDto is a pure deserialization — do not silently modify parameter values here.
        // Budget-driven iteration recalculation is handled explicitly by AlgorithmFactory.
        private static SimulatedAnnealingParameters FromDto(SimulatedAnnealingParametersDto dto)
        {
            var coolingName = dto.CoolingSchedule ?? ExponentialCoolingSchedule.Name;
            return new SimulatedAnnealingParameters
            {
                Seed = dto.Seed,
                Iterations = dto.Iterations,
                EvaluationBudget = dto.EvaluationBudget,
                InitialTemperature = dto.InitialTemperature,
                CoolingRate = dto.CoolingRate,
                MinimumTemperature = dto.MinimumTemperature,
                NeighborhoodOperator = ResolveOperator(dto.NeighborhoodOperator ?? SwapNeighborhood.Name, NeighborhoodRegistry, NeighborhoodOperatorName),
                AcceptanceFunction = ResolveOperator(dto.AcceptanceFunction ?? ProbabilisticAcceptanceFunction.Name, AcceptanceRegistry, AcceptanceFunctionName),
                CoolingSchedule = ResolveOperator(coolingName, CoolingRegistry, CoolingScheduleName),
                CoolingScheduleParameters = DeserializeCoolingScheduleParameters(coolingName, dto.CoolingScheduleParameters),
                Monitoring = dto.Monitoring
            };
        }

        private static T ResolveOperator<T>(string name, Dictionary<string, Func<T>> registry, string parameterName) where T : class
        {
            if (!registry.TryGetValue(name, out var found))
                throw new ArgumentException(
                    $"Unknown {parameterName} '{name}'. Valid values: {string.Join(", ", registry.Keys)}");
            return found();
        }

        private static JsonElement SerializeCoolingScheduleParameters(string coolingScheduleName, ICoolingScheduleParameters parameters)
        {
            return coolingScheduleName switch
            {
                var n when n == LinearCoolingSchedule.Name && parameters is LinearCoolingScheduleParameters linear
                    => JsonSerializer.SerializeToElement(linear, JsonOptions),
                var n when n == ExponentialCoolingSchedule.Name && parameters is ExponentialCoolingScheduleParameters exponential
                    => JsonSerializer.SerializeToElement(exponential, JsonOptions),
                var n when n == LinearCoolingSchedule.Name
                    => throw new ArgumentException($"{nameof(LinearCoolingSchedule)} requires {nameof(LinearCoolingScheduleParameters)}."),
                var n when n == ExponentialCoolingSchedule.Name
                    => throw new ArgumentException($"{nameof(ExponentialCoolingSchedule)} requires {nameof(ExponentialCoolingScheduleParameters)}."),
                _ => throw new ArgumentException($"Unknown cooling schedule '{coolingScheduleName}'.")
            };
        }

        private static ICoolingScheduleParameters DeserializeCoolingScheduleParameters(string coolingScheduleName, JsonElement json)
        {
            if (coolingScheduleName == LinearCoolingSchedule.Name)
                return DeserializeOrDefault<LinearCoolingScheduleParameters>(json);
            if (coolingScheduleName == ExponentialCoolingSchedule.Name)
                return DeserializeOrDefault<ExponentialCoolingScheduleParameters>(json);
            throw new ArgumentException($"Unknown cooling schedule '{coolingScheduleName}'.");
        }

        private static T DeserializeOrDefault<T>(JsonElement json) where T : ICoolingScheduleParameters, new()
        {
            if (json.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
                return new T();

            return JsonSerializer.Deserialize<T>(json.GetRawText(), JsonOptions) ?? new T();
        }

        private static bool IsCoolingParametersCompatible(string coolingScheduleName, ICoolingScheduleParameters parameters)
        {
            if (coolingScheduleName == LinearCoolingSchedule.Name) return parameters is LinearCoolingScheduleParameters;
            if (coolingScheduleName == ExponentialCoolingSchedule.Name) return parameters is ExponentialCoolingScheduleParameters;
            return false;
        }

        private static (string Name, object? Value)[] BuildOutputFields(SimulatedAnnealingParametersDto dto) =>
        [
            (SeedName, dto.Seed),
            (IterationsName, dto.Iterations),
            (EvaluationBudgetName, dto.EvaluationBudget),
            (InitialTemperatureName, dto.InitialTemperature),
            (CoolingRateName, dto.CoolingRate),
            (MinimumTemperatureName, dto.MinimumTemperature),
            (NeighborhoodOperatorName, dto.NeighborhoodOperator),
            (AcceptanceFunctionName, dto.AcceptanceFunction),
            (CoolingScheduleName, dto.CoolingSchedule)
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
