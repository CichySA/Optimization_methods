using System;
using System.Collections.Generic;
using System.Globalization;
using PFSP.Monitoring;
using System.Text.Json;

namespace PFSP.Algorithms.RandomSearch
{
    public static class RandomParameterFactory
    {
        public const string SeedName = "Seed";
        public const string IterationsName = "Iterations";
        public const string TimeLimitMsName = "TimeLimitMs";
        public const string EvaluationBudgetName = "EvaluationBudget";

        // Validate the constructed parameters and collect all violations before throwing.
        public static void Validate(RandomSearchParameters p)
        {
            var errors = new List<string>();

            if (p is null)
                throw new ArgumentNullException(nameof(p));

            if (p.Seed < 0)
                errors.Add($"{SeedName} must be non-negative (0 means random). Actual: {p.Seed}.");

            if (p.TimeLimit.HasValue && p.Iterations > 0)
                errors.Add($"Both {IterationsName} and {TimeLimitMsName} cannot be specified at the same time. Iterations: {p.Iterations}, TimeLimit: {p.TimeLimit}.");

            if (!p.TimeLimit.HasValue && p.Iterations <= 0)
                errors.Add($"{IterationsName} must be greater than zero when TimeLimit is not provided. Actual: {p.Iterations}.");

            if (p.TimeLimit.HasValue && p.TimeLimit.Value <= TimeSpan.Zero)
                errors.Add($"{TimeLimitMsName} must be greater than zero when specified. Actual: {p.TimeLimit}.");

            if (errors.Count > 0)
                throw new ArgumentException($"Invalid {nameof(RandomSearchParameters)}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }

        // DTO used by ExperimentRunner and for JSON deserialization.
        public sealed class RandomParametersDto
        {
            public int Seed { get; set; } = 0;
            public int Iterations { get; set; } = 100;
            public int? TimeLimitMs { get; set; }
            public long? EvaluationBudget { get; set; }
            public AlgorithmMonitoringOptions? Monitoring { get; set; }
        }

        private static readonly Dictionary<string, string> ParameterFormatMapping = new()
        {
            { SeedName, "D" },
            { IterationsName, "D" },
            { TimeLimitMsName, "D" },
            { EvaluationBudgetName, "D" }
        };

        public static string ToName(RandomSearchParameters p)
        {
            var dto = ToDto(p);
            var fields = BuildOutputFields(dto);
            return "Random_" + string.Join("_", fields.Select(f => $"{f.Name}{FormatValue(f.Name, f.Value, ParameterFormatMapping)}"));
        }

        private static RandomParametersDto ToDto(RandomSearchParameters p) => new()
        {
            Seed = p.Seed,
            Iterations = p.Iterations,
            TimeLimitMs = p.TimeLimit.HasValue ? (int?)CheckedMilliseconds(p.TimeLimit.Value) : null,
            EvaluationBudget = p.EvaluationBudget > 0 ? p.EvaluationBudget : null,
            Monitoring = p.Monitoring
        };

        private static int CheckedMilliseconds(TimeSpan ts)
        {
            // Cap/convert to int with checking to avoid overflow
            var ms = (long)ts.TotalMilliseconds;
            if (ms > int.MaxValue) return int.MaxValue;
            if (ms < int.MinValue) return int.MinValue;
            return (int)ms;
        }

        private static (string Name, object? Value)[] BuildOutputFields(RandomParametersDto dto) =>
            [
                (IterationsName, dto.Iterations),
                (TimeLimitMsName, dto.TimeLimitMs),
                (EvaluationBudgetName, dto.EvaluationBudget),
                (SeedName, dto.Seed)
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
