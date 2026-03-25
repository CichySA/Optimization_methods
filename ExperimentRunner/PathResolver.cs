namespace ExperimentRunner
{
    public static class PathResolver
    {
        public static string ResolveSolutionDirectory()
        {
            var baseDir = AppContext.BaseDirectory ?? Environment.CurrentDirectory;
            return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        }

        public static string ResolveExperimentsDirectory()
            => Path.Combine(ResolveSolutionDirectory(), "Experiments");

        public static string ResolveExperimentDirectory(string experimentName)
        {
            if (string.IsNullOrWhiteSpace(experimentName))
                throw new ArgumentException("Experiment name is null or empty.", nameof(experimentName));

            return Path.Combine(ResolveExperimentsDirectory(), experimentName.Trim());
        }

        public static string ResolveExperimentConfigPath(string configPathOrExperimentName)
        {
            if (string.IsNullOrWhiteSpace(configPathOrExperimentName))
                throw new ArgumentException("Configuration path is null or empty.", nameof(configPathOrExperimentName));

            var trimmed = configPathOrExperimentName.Trim();
            if (File.Exists(trimmed))
                return Path.GetFullPath(trimmed);

            if (Directory.Exists(trimmed))
                return Path.Combine(Path.GetFullPath(trimmed), "experimentrunner.json");

            if (!Path.IsPathRooted(trimmed) && Path.GetExtension(trimmed).Length == 0)
                return Path.Combine(ResolveExperimentDirectory(trimmed), "experimentrunner.json");

            return Path.GetFullPath(trimmed);
        }

        public static string NormalizeInstanceIdentifier(string baseNameOrPath)
        {
            if (string.IsNullOrWhiteSpace(baseNameOrPath))
                throw new ArgumentException("Instance identifier is null or empty.", nameof(baseNameOrPath));

            var normalized = Path.GetFileNameWithoutExtension(baseNameOrPath.Trim());
            if (string.IsNullOrWhiteSpace(normalized))
                throw new ArgumentException("Instance identifier is invalid.", nameof(baseNameOrPath));

            return normalized;
        }

        public static string ResolveOutputDirectory(string configuredOutDir)
        {
            if (string.IsNullOrWhiteSpace(configuredOutDir))
                throw new ArgumentException("Output directory is null or empty.", nameof(configuredOutDir));

            var outDir = Path.IsPathRooted(configuredOutDir)
                ? Path.GetFullPath(configuredOutDir)
                : Path.GetFullPath(Path.Combine(ResolveSolutionDirectory(), configuredOutDir));

            Directory.CreateDirectory(outDir);
            return outDir;
        }

        public static string ResolveOutputFilePath(string outDir, string fileName)
        {
            if (string.IsNullOrWhiteSpace(outDir))
                throw new ArgumentException("Output directory is null or empty.", nameof(outDir));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is null or empty.", nameof(fileName));

            return Path.Combine(outDir, fileName);
        }
    }
}
