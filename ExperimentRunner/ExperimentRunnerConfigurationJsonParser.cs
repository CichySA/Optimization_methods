using System.Text.Json;

namespace ExperimentRunner
{
    public static class ExperimentRunnerConfigurationJsonParser
    {
        private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

        public static ExperimentRunnerConfiguration Load(string pathOrExperimentName)
        {
            var candidatePath = Path.IsPathRooted(pathOrExperimentName)
                ? pathOrExperimentName
                : Path.Combine(PathResolver.ResolveSolutionDirectory(), pathOrExperimentName);

            var resolvedPath = PathResolver.ResolveExperimentConfigPath(candidatePath);
            if (!File.Exists(resolvedPath))
                throw new FileNotFoundException($"Config file not found: {resolvedPath}", resolvedPath);

            var json = File.ReadAllText(resolvedPath);
            var loaded = JsonSerializer.Deserialize<ExperimentRunnerConfiguration>(json, Options)
                   ?? ExperimentRunnerConfiguration.Default;

            var derivedName = PathResolver.ResolveExperimentNameFromConfigPath(resolvedPath);
            var configuredName = PathResolver.ToSafeFileName(loaded.Name ?? string.Empty);
            var effectiveName = string.Equals(configuredName, "unnamed_experiment", StringComparison.Ordinal)
                ? derivedName
                : configuredName;

            return loaded with
            {
                ConfigPath = resolvedPath,
                ExperimentName = effectiveName
            };
        }
    }
}
