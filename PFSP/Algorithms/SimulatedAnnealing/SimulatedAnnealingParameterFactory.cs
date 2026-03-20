using System.Globalization;
using System.Text;
using System.Text.Json;
using PFSP.Algorithms.SimulatedAnnealing.Operators;

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

        public static readonly Dictionary<string, Func<INeighborhoodOperator>> NeighborhoodRegistry = new(StringComparer.OrdinalIgnoreCase)
        {
            { SwapNeighborhoodName, static () => new SwapNeighborhood() },
            { InsertNeighborhoodName, static () => new InsertNeighborhood() },
            { ReverseNeighborhoodName, static () => new ReverseNeighborhood() }
        };

        public static readonly Dictionary<string, Func<IAcceptanceFunction>> AcceptanceRegistry = new(StringComparer.OrdinalIgnoreCase)
        {
            { ProbabilisticAcceptanceName, static () => new ProbabilisticAcceptanceAlgorithm() }
            ,{ ThresholdAcceptanceName, static () => new ThresholdAcceptanceFunction() }
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
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = JsonSerializer.Deserialize<SimulatedAnnealingParametersDto>(json, options) ?? new SimulatedAnnealingParametersDto();
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
        }

        private static SimulatedAnnealingParametersDto ToDto(SimulatedAnnealingParameters p) => new()
        {
            Seed = p.Seed,
            Iterations = p.Iterations,
            InitialTemperature = p.InitialTemperature,
            CoolingRate = p.CoolingRate,
            MinimumTemperature = p.MinimumTemperature,
            NeighborhoodOperator = ResolveOperatorName(p.NeighborhoodOperator, NeighborhoodRegistry),
            AcceptanceFunction = ResolveOperatorName(p.AcceptanceFunction, AcceptanceRegistry),
            CoolingSchedule = ResolveOperatorName(p.CoolingSchedule, CoolingRegistry)
        };

        private static SimulatedAnnealingParameters FromDto(SimulatedAnnealingParametersDto dto) => new()
        {
            Seed = dto.Seed,
            Iterations = dto.Iterations,
            InitialTemperature = dto.InitialTemperature,
            CoolingRate = dto.CoolingRate,
            MinimumTemperature = dto.MinimumTemperature,
            NeighborhoodOperator = ResolveOperator(dto.NeighborhoodOperator, NeighborhoodRegistry, NeighborhoodOperatorName),
            AcceptanceFunction = ResolveOperator(dto.AcceptanceFunction, AcceptanceRegistry, AcceptanceFunctionName),
            CoolingSchedule = ResolveOperator(dto.CoolingSchedule, CoolingRegistry, CoolingScheduleName)
        };

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
