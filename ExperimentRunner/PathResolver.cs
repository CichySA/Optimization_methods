namespace ExperimentRunner
{
    public static class PathResolver
    {
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
            var baseDir = AppContext.BaseDirectory ?? Environment.CurrentDirectory;
            var solutionDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));

            var outDir = Path.IsPathRooted(configuredOutDir)
                ? configuredOutDir
                : Path.Combine(solutionDir, configuredOutDir);

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
