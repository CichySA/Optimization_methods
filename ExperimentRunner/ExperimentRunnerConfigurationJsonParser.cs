using System.Text.Json;

namespace ExperimentRunner
{
    public static class ExperimentRunnerConfigurationJsonParser
    {
        private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

        public static ExperimentRunnerConfiguration Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Config file not found: {path}", path);

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ExperimentRunnerConfiguration>(json, Options)
                   ?? ExperimentRunnerConfiguration.Default;
        }
    }
}
