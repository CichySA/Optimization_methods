using System.Globalization;
using System.Text;
using System.Text.Json;
using PFSP.Algorithms.Monitoring;
using PFSP.Algorithms.SimulatedAnnealing.Operators;
using PFSP.Algorithms.SimulatedAnnealing.Operators.AcceptanceFunctions;
using PFSP.Algorithms.SimulatedAnnealing.Operators.CoolingSchedules;
using PFSP.Algorithms.SimulatedAnnealing.Operators.NeighborhoodOperators;

namespace PFSP.Algorithms.SimulatedAnnealing
{
    public static class SimulatedAnnealingParameterFactory
    {
        public const string SeedName = "Seed";
        public const string IterationsName = "Iterations";
        public const string InitialTemperatureName = "InitialTemperature";
        public const string CoolingRateName = "CoolingRate";
        public const string MinimumTemperatureName = "MinimumTemperature";
        public const string NeighborhoodOperatorName = "NeighborhoodOperator";
        public const string AcceptanceFunctionName = "AcceptanceFunction";
        public const string CoolingScheduleName = "CoolingSchedule";

        public const string SwapNeighborhoodName = "Swap";
        public const string InsertNeighborhoodName = "Insert";
        public const string ReverseNeighborhoodName = "Reverse";
        public const string ProbabilisticAcceptanceName = "Probabilistic";
        public const string ThresholdAcceptanceName = "Threshold";
        public const string ExponentialCoolingName = "Exponential";
        public const string LinearCoolingName = "Linear";

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public static readonly Dictionary<string, Func<INeighborhoodOperator>> NeighborhoodRegistry = new(StringComparer.OrdinalIgnoreCase)
        {
            { SwapNeighborhoodName, static () => new SwapNeighborhood() },
            { InsertNeighborhoodName, static () => new InsertNeighborhood() },
            { ReverseNeighborhoodName, static () => new ReverseNeighborhood() }
        };

        public static readonly Dictionary<string, Func<IAcceptanceFunction>> AcceptanceRegistry = new(StringComparer.OrdinalIgnoreCase)
        {
            { ProbabilisticAcceptanceName, static () => new ProbabilisticAcceptanceFunction() },
            { ThresholdAcceptanceName, static () => new ThresholdAcceptanceFunction() }
        };

        public static readonly Dictionary<string, Func<ICoolingSchedule>> CoolingRegistry = new(StringComparer.OrdinalIgnoreCase)
        {
            { ExponentialCoolingName, static () => new ExponentialCoolingSchedule() },
            { LinearCoolingName, static () => new LinearCoolingSchedule() }
        };

        private static readonly Dictionary<string, string> ParameterFormatMapping = new()
        {
            { SeedName, "D" },
            { IterationsName, "D" },
            { InitialTemperatureName, "F2" },
            { CoolingRateName, "F4" },
            { MinimumTemperatureName, "F4" },
            { NeighborhoodOperatorName, "S" },
            { AcceptanceFunctionName, "S" },
            { CoolingScheduleName, "S" }
        };

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

            var coolingName = ResolveOperatorName(p.CoolingSchedule, CoolingRegistry);
            if (!IsCoolingParametersCompatible(coolingName, p.CoolingScheduleParameters))
            {
                errors.Add($"CoolingScheduleParameters type '{p.CoolingScheduleParameters.GetType().Name}' is not compatible with CoolingSchedule '{coolingName}'.");
            }

            if (errors.Count > 0)
                throw new ArgumentException($"Invalid {nameof(SimulatedAnnealingParameters)}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }

        private sealed class SimulatedAnnealingParametersDto
        {
            public int Seed { get; set; } = SimulatedAnnealingParameters.DefaultSeed;
            public int Iterations { get; set; } = SimulatedAnnealingParameters.DefaultIterations;
            public double InitialTemperature { get; set; } = SimulatedAnnealingParameters.DefaultInitialTemperature;
            public double CoolingRate { get; set; } = SimulatedAnnealingParameters.DefaultCoolingRate;
            public double MinimumTemperature { get; set; } = SimulatedAnnealingParameters.DefaultMinimumTemperature;
            public string? NeighborhoodOperator { get; set; } = SwapNeighborhoodName;
            public string? AcceptanceFunction { get; set; } = ProbabilisticAcceptanceName;
            public string? CoolingSchedule { get; set; } = ExponentialCoolingName;
            public JsonElement CoolingScheduleParameters { get; set; }
            public AlgorithmMonitoringOptions Monitoring { get; set; } = new();
        }

        private static SimulatedAnnealingParametersDto ToDto(SimulatedAnnealingParameters p)
        {
            var coolingName = ResolveOperatorName(p.CoolingSchedule, CoolingRegistry);
            return new SimulatedAnnealingParametersDto
            {
                Seed = p.Seed,
                Iterations = p.Iterations,
                InitialTemperature = p.InitialTemperature,
                CoolingRate = p.CoolingRate,
                MinimumTemperature = p.MinimumTemperature,
                NeighborhoodOperator = ResolveOperatorName(p.NeighborhoodOperator, NeighborhoodRegistry),
                AcceptanceFunction = ResolveOperatorName(p.AcceptanceFunction, AcceptanceRegistry),
                CoolingSchedule = coolingName,
                CoolingScheduleParameters = SerializeCoolingScheduleParameters(coolingName, p.CoolingScheduleParameters),
                Monitoring = p.Monitoring
            };
        }

        private static SimulatedAnnealingParameters FromDto(SimulatedAnnealingParametersDto dto)
        {
            var coolingName = dto.CoolingSchedule ?? ExponentialCoolingName;
            return new SimulatedAnnealingParameters
            {
                Seed = dto.Seed,
                Iterations = dto.Iterations,
                InitialTemperature = dto.InitialTemperature,
                CoolingRate = dto.CoolingRate,
                MinimumTemperature = dto.MinimumTemperature,
                NeighborhoodOperator = ResolveOperator(dto.NeighborhoodOperator, NeighborhoodRegistry, NeighborhoodOperatorName),
                AcceptanceFunction = ResolveOperator(dto.AcceptanceFunction, AcceptanceRegistry, AcceptanceFunctionName),
                CoolingSchedule = ResolveOperator(coolingName, CoolingRegistry, CoolingScheduleName),
                CoolingScheduleParameters = DeserializeCoolingScheduleParameters(coolingName, dto.CoolingScheduleParameters),
                Monitoring = dto.Monitoring
            };
        }

        private static JsonElement SerializeCoolingScheduleParameters(string coolingScheduleName, ICoolingScheduleParameters parameters)
        {
            return coolingScheduleName switch
            {
                LinearCoolingName when parameters is LinearCoolingScheduleParameters linear
                    => JsonSerializer.SerializeToElement(linear, JsonOptions),
                ExponentialCoolingName when parameters is ExponentialCoolingScheduleParameters exponential
                    => JsonSerializer.SerializeToElement(exponential, JsonOptions),
                LinearCoolingName
                    => throw new ArgumentException($"{nameof(LinearCoolingSchedule)} requires {nameof(LinearCoolingScheduleParameters)}."),
                ExponentialCoolingName
                    => throw new ArgumentException($"{nameof(ExponentialCoolingSchedule)} requires {nameof(ExponentialCoolingScheduleParameters)}."),
                _ => throw new ArgumentException($"Unknown cooling schedule '{coolingScheduleName}'.")
            };
        }

        private static ICoolingScheduleParameters DeserializeCoolingScheduleParameters(string coolingScheduleName, JsonElement json)
        {
            return coolingScheduleName switch
            {
                LinearCoolingName => DeserializeOrDefault<LinearCoolingScheduleParameters>(json),
                ExponentialCoolingName => DeserializeOrDefault<ExponentialCoolingScheduleParameters>(json),
                _ => throw new ArgumentException($"Unknown cooling schedule '{coolingScheduleName}'.")
            };
        }

        private static T DeserializeOrDefault<T>(JsonElement json) where T : ICoolingScheduleParameters, new()
        {
            if (json.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
                return new T();

            return JsonSerializer.Deserialize<T>(json.GetRawText(), JsonOptions) ?? new T();
        }

        private static bool IsCoolingParametersCompatible(string coolingScheduleName, ICoolingScheduleParameters parameters)
        {
            return coolingScheduleName switch
            {
                LinearCoolingName => parameters is LinearCoolingScheduleParameters,
                ExponentialCoolingName => parameters is ExponentialCoolingScheduleParameters,
                _ => false
            };
        }

        private static string ResolveOperatorName<T>(T method, Dictionary<string, Func<T>> registry) where T : class
        {
            var methodType = method.GetType();
            foreach (var item in registry)
                if (methodType == item.Value().GetType()) return item.Key;

            throw new ArgumentException($"Unknown operator instance of type {methodType.Name}. No matching type found in registry.");
        }

        private static T ResolveOperator<T>(string? name, Dictionary<string, Func<T>> registry, string parameterName) where T : class
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"{parameterName} must be specified. Valid values: {string.Join(", ", registry.Keys)}");

            if (!registry.TryGetValue(name, out var found))
                throw new ArgumentException($"Unknown {parameterName} '{name}'. Valid values: {string.Join(", ", registry.Keys)}");

            return found();
        }

        private static (string Name, object? Value)[] BuildOutputFields(SimulatedAnnealingParametersDto dto) =>
        [
            (SeedName, dto.Seed),
            (IterationsName, dto.Iterations),
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
