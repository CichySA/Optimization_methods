using System.IO;
using Newtonsoft.Json;

namespace ExperimentRunner
{
    public static class ResultSaver
    {
        public static void AppendCsvLine(string outDir, string fileName, string line)
        {
            var path = Path.Combine(outDir, fileName);
            File.AppendAllText(path, line + System.Environment.NewLine);
        }

        public static void SaveJson(string outDir, string fileName, object data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(Path.Combine(outDir, fileName), json);
        }
    }
}
