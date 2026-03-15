using PFSP.Evaluators;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PFSP.Instances
{
    // InstanceReader reads embedded Taillard instances from resources folder
    public static class InstanceReader
    {
        public static Instance Read(int jobs, int machines, int instanceNumber)
        {
            var fileName = $"tai{jobs}_{machines}_{instanceNumber}.fsp";
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)) ?? throw new FileNotFoundException($"Instance file {fileName} not found in resources.");
            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException($"Instance resource {resourceName} not found.");
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            // extract integers and treat '-' placeholders as 0
            var matches = Regex.Matches(content, "-|\\d+");
            var numbers = matches.Cast<Match>()
                .Select(m => m.Value == "-" ? 0 : int.Parse(m.Value))
                .ToArray();
            if (numbers.Length < 5) throw new InvalidDataException("Invalid instance file format");

            var data = numbers.Skip(5).Select(n => (double)n).ToArray();
            var matrix = new double[machines, jobs];
            for (int m = 0; m < machines; m++)
            {
                for (int j = 0; j < jobs; j++)
                {
                    matrix[m, j] = data[m * jobs + j];
                }
            }

            var instance = Instance.Create(matrix, new TotalFlowTimeEvaluator(), numbers[2], numbers[3], numbers[4]);

            return instance;
        }

        // Overload to read from a base name like "tai_20_5_0" or "tai20_5_0"
        public static Instance Read(string baseName)
        {
            if (string.IsNullOrWhiteSpace(baseName)) throw new ArgumentException("baseName is null or empty", nameof(baseName));
            var nums = Regex.Matches(baseName, "\\d+")
                .Cast<Match>()
                .Select(m => int.Parse(m.Value))
                .ToArray();
            if (nums.Length < 3) throw new FormatException($"Could not parse jobs,machines,instance from '{baseName}'.");
            return Read(nums[0], nums[1], nums[2]);
        }
    }
}
