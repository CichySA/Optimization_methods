using System.Text;
using PFSP.Algorithms;
using PFSP.Instances;
using PFSP.Solutions;
using PFSP.Algorithms.Random;
using PFSP.Solutions.PermutationSolutionGenerators;
using Newtonsoft.Json;

namespace ExperimentRunner
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Experiment Runner");

            // Simple configuration: compare RandomAlgorithm with different sample counts/seeds
            var algorithms = new List<(string Name, IAlgorithm Algo, IParameters Params)>
            {
                ("Random_1000_s123", new RandomAlgorithm(), RandomParameters.ForRuns(1000, seed:123)),
                ("Random_10000_s123", new RandomAlgorithm(), RandomParameters.ForRuns(10000, seed:123)),
                ("Random_1000_s0", new RandomAlgorithm(), RandomParameters.ForRuns(1000, seed:0)),
            };

            // Create a simple random instance for experiments (same as benchmark)
            int jobs = 100;
            int machines = 10;
            var instance = new Instance
            {
                Jobs = jobs,
                Machines = machines,
                Matrix = new double[machines, jobs],
                Evaluator = new PFSP.Evaluators.TotalFlowTimeEvaluator()
            };
            var rnd = new Random(123);
            for (int m = 0; m < machines; m++)
                for (int j = 0; j < jobs; j++)
                    instance.Matrix[m, j] = rnd.Next(1, 100);

            // Experiment parameters
            int repeats = 5;
            var results = new List<object>();
            var outDir = Path.Combine(Environment.CurrentDirectory, "experiment_results");
            Directory.CreateDirectory(outDir);

            for (int r = 0; r < repeats; r++)
            {
                Console.WriteLine($"Repeat {r+1}/{repeats}");
                foreach (var (name, algo, pars) in algorithms)
                {
                    Console.WriteLine($" Running {name}...");
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var result = algo.Solve(instance, pars);
                    sw.Stop();

                    var record = new
                    {
                        Algorithm = name,
                        Params = pars.GetType().Name,
                        Seed = pars is RandomParameters rp ? rp.Seed : (int?)null,
                        BestCost = (result.Best as PermutationSolution)?.Cost,
                        Evaluations = result.Evaluations,
                        ElapsedMs = sw.Elapsed.TotalMilliseconds,
                        BestFoundAt = result.BestFoundAtEvaluation,
                        Timestamp = DateTimeOffset.UtcNow
                    };
                    results.Add(record);

                    // Append CSV line
                    var csvLine = new StringBuilder();
                    if (r == 0 && results.Count == 1)
                    {
                        var header = "Algorithm,Params,Seed,BestCost,Evaluations,ElapsedMs,BestFoundAt,Timestamp";
                        File.AppendAllText(Path.Combine(outDir, "results.csv"), header + Environment.NewLine);
                    }
                    csvLine.Append($"{record.Algorithm},{record.Params},{record.Seed},{record.BestCost},{record.Evaluations},{record.ElapsedMs},{record.BestFoundAt},{record.Timestamp:o}");
                    File.AppendAllText(Path.Combine(outDir, "results.csv"), csvLine + Environment.NewLine);
                }
            }

            // Save full JSON
            var json = JsonConvert.SerializeObject(results, Formatting.Indented);
            File.WriteAllText(Path.Combine(outDir, $"results_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json"), json);

            Console.WriteLine("Done. Results saved to " + outDir);
        }
    }
}
