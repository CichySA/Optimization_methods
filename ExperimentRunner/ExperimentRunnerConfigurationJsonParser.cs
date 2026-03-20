using System.Text.Json;

namespace ExperimentRunner
{
    public static class ExperimentRunnerConfigurationJsonParser
    {
        private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

        public static ExperimentRunnerConfiguration Load(string pathOrExperimentName)
        {
            var resolvedPath = PathResolver.ResolveExperimentConfigPath(pathOrExperimentName);
            if (!File.Exists(resolvedPath))
                throw new FileNotFoundException($"Config file not found: {resolvedPath}", resolvedPath);

            var json = File.ReadAllText(resolvedPath);
            return JsonSerializer.Deserialize<ExperimentRunnerConfiguration>(json, Options)
                   ?? ExperimentRunnerConfiguration.Default;
        }
    }
}
