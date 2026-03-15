using System;

namespace ExperimentRunner
{
    public static class Visualizer
    {
        public static void DisplayRunStart(string instanceName, string algorithmName, int? seed)
        {
            if (seed.HasValue)
                Console.WriteLine($"Running {algorithmName} on {instanceName} (seed {seed.Value})...");
            else
                Console.WriteLine($"Running {algorithmName} on {instanceName}...");
        }

        public static void DisplayLine(string line)
        {
            Console.WriteLine(line);
        }
    }
}
