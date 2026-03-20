using Newtonsoft.Json;

namespace ExperimentRunner
{
    public static class ResultSaver
    {
        public static string ResolveOutputDirectory(string configuredOutDir)
            => PathResolver.ResolveOutputDirectory(configuredOutDir);

        public static void SaveCsv(string outDir, string fileName, string header, IReadOnlyList<string> lines)
        {
            var allLines = new string[lines.Count + 1];
            allLines[0] = header;
            for (var i = 0; i < lines.Count; i++)
                allLines[i + 1] = lines[i];

            File.WriteAllLines(PathResolver.ResolveOutputFilePath(outDir, fileName), allLines);
        }

        public static void AppendCsvLine(string outDir, string fileName, string line)
        {
            File.AppendAllText(PathResolver.ResolveOutputFilePath(outDir, fileName), line + System.Environment.NewLine);
        }

        public static void SaveJson(string outDir, string fileName, object data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(PathResolver.ResolveOutputFilePath(outDir, fileName), json);
        }
    }
}
