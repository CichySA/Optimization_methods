using System.Globalization;

namespace ExperimentRunner
{
    public static class ExperimentRunnerConfigurationCliParser
    {
        private const string HelpText = """
Usage:
  ExperimentRunner --config <path>

Options:
  --config <path>   Path to the JSON configuration file.
  --help            Show this help text.

Example:
  dotnet run --project ExperimentRunner -- --config experimentrunner.json
""";

        public static ExperimentRunnerConfiguration? Parse(string[] args)
        {
            if (args.Length == 0 ||
                args.Any(a => string.Equals(a, "--help", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(a, "-h", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine(HelpText);
                return null;
            }

            string? configPath = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], "--config", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                        throw new ArgumentException("--config requires a path value.");
                    configPath = args[++i];
                }
                else
                {
                    throw new ArgumentException($"Unknown argument '{args[i]}'. Use --help for usage.");
                }
            }

            if (configPath is null)
            {
                Console.WriteLine(HelpText);
                return null;
            }

            return ExperimentRunnerConfigurationJsonParser.Load(configPath);
        }
    }
}
