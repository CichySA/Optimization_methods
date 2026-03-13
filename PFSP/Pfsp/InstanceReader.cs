using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;

namespace PFSP
{
    // InstanceReader reads embedded Taillard instances from resources folder
    public static class InstanceReader
    {
        public static Instance Read(int jobs, int machines, int instanceNumber)
        {
            var fileName = $"tai{jobs}_{machines}_{instanceNumber}.fsp";
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
                throw new FileNotFoundException($"Instance file {fileName} not found in embedded resources.");

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException($"Instance resource {resourceName} not found.");
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            // extract integers and treat '-' placeholders as 0
            var matches = Regex.Matches(content, "-|\\d+");
            var numbers = matches.Cast<Match>()
                .Select(m => m.Value == "-" ? 0 : int.Parse(m.Value))
                .ToArray();
            if (numbers.Length < 5) throw new InvalidDataException("Invalid instance file format");

            var instance = new Instance();
            instance.Jobs = numbers[0];
            instance.Machines = numbers[1];
            instance.Seed = numbers[2];
            instance.UpperBound = numbers[3];
            instance.LowerBound = numbers[4];

            var data = numbers.Skip(5).Select(n => (double)n).ToArray();
            instance.Matrix = new double[instance.Machines, instance.Jobs];
            for (int m = 0; m < instance.Machines; m++)
            {
                for (int j = 0; j < instance.Jobs; j++)
                {
                    instance.Matrix[m, j] = data[m * instance.Jobs + j];
                }
            }

            return instance;
        }
    }
}
