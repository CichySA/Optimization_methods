using System.Globalization;

namespace ExperimentRunner
{
    public static class ExperimentRunnerConfigurationCliParser
    {
        private const string HelpText = """
Usage:
  ExperimentRunner [--config path] [--instances a b c] [--samples 1 10 100] [--seed 123] [--outdir path]

Examples:
  dotnet run --project ExperimentRunner -- --instances tai_20_5_0 --samples 10 100 --seed 42
  dotnet run --project ExperimentRunner -- --config experimentrunner.json
""";

        public static ExperimentRunnerConfiguration? Parse(string[] args)
        {
            if (args.Any(a => string.Equals(a, "--help", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "-h", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine(HelpText);
                return null;
            }

            string? configPath = null;
            var instances = new List<string>();
            var samples = new List<int>();
            int? seed = null;
            string? outDir = null;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (IsOption(arg, "--config"))
                {
                    configPath = RequireValue(args, ref i, arg);
                    continue;
                }

                if (IsOption(arg, "--instances"))
                {
                    CollectStrings(args, ref i, instances, arg);
                    continue;
                }

                if (IsOption(arg, "--samples"))
                {
                    CollectInts(args, ref i, samples, arg);
                    continue;
                }

                if (IsOption(arg, "--seed"))
                {
                    seed = ParseInt(RequireValue(args, ref i, arg), arg);
                    continue;
                }

                if (IsOption(arg, "--outdir"))
                {
                    outDir = RequireValue(args, ref i, arg);
                    continue;
                }

                throw new ArgumentException($"Unknown argument '{arg}'. Use --help for usage.");
            }

            var baseConfig = string.IsNullOrWhiteSpace(configPath)
                ? ExperimentRunnerConfiguration.Default
                : ExperimentRunnerConfigurationJsonParser.Load(configPath);

            return baseConfig with
            {
                Instances = instances.Count > 0 ? instances.ToArray() : baseConfig.Instances,
                Samples = samples.Count > 0 ? samples.ToArray() : baseConfig.Samples,
                Seed = seed ?? baseConfig.Seed,
                OutDir = !string.IsNullOrWhiteSpace(outDir) ? outDir : baseConfig.OutDir
            };
        }

        private static bool IsOption(string arg, string name) => string.Equals(arg, name, StringComparison.OrdinalIgnoreCase);

        private static string RequireValue(string[] args, ref int index, string optionName)
        {
            if (index + 1 >= args.Length)
                throw new ArgumentException($"Option '{optionName}' requires a value.");

            index++;
            return args[index];
        }

        private static void CollectStrings(string[] args, ref int index, List<string> values, string optionName)
        {
            int start = index + 1;
            while (index + 1 < args.Length && !IsOption(args[index + 1]))
            {
                index++;
                values.Add(args[index]);
            }

            if (values.Count == 0 || index < start)
                throw new ArgumentException($"Option '{optionName}' requires at least one value.");
        }

        private static void CollectInts(string[] args, ref int index, List<int> values, string optionName)
        {
            int start = index + 1;
            while (index + 1 < args.Length && !IsOption(args[index + 1]))
            {
                index++;
                values.Add(ParseInt(args[index], optionName));
            }

            if (values.Count == 0 || index < start)
                throw new ArgumentException($"Option '{optionName}' requires at least one value.");
        }

        private static bool IsOption(string arg) => arg.StartsWith("--", StringComparison.Ordinal) || arg.StartsWith("-", StringComparison.Ordinal);

        private static int ParseInt(string value, string optionName)
        {
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                throw new ArgumentException($"Option '{optionName}' requires an integer value, but '{value}' was provided.");

            return parsed;
        }
    }
}
