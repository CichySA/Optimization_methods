using PFSP.Evaluators;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PFSP.Instances
{
    // InstanceReader reads embedded Taillard/custom instances.
    public static class InstanceReader
    {
        private static readonly string[] PreferredResourceMarkers =
        [
            "Instances.Taillard",
            "Instances.Custom"
        ];

        public static Instance Read(int jobs, int machines, int instanceNumber)
        {
            var fileName = $"tai{jobs}_{machines}_{instanceNumber}.fsp";
            var content = ReadEmbeddedResource(fileName);

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

        private static string ReadEmbeddedResource(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("fileName is null or empty.", nameof(fileName));

            var assembly = typeof(InstanceReader).Assembly;
            var resourceNames = assembly.GetManifestResourceNames();

            foreach (var marker in PreferredResourceMarkers)
            {
                var resourceName = resourceNames
                    .FirstOrDefault(n => n.Contains(marker, StringComparison.OrdinalIgnoreCase) && n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

                if (resourceName is null)
                    continue;

                using var stream = assembly.GetManifestResourceStream(resourceName)
                    ?? throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }

            var fallback = resourceNames.FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
            if (fallback is not null)
            {
                using var stream = assembly.GetManifestResourceStream(fallback)
                    ?? throw new FileNotFoundException($"Embedded resource '{fallback}' not found.");
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }

            throw new FileNotFoundException($"Embedded instance file {fileName} not found in resources.");
        }
    }
}
