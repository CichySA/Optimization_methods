using ExperimentRunner;
using PFSP.Algorithms.Random;
using PFSP.Solutions;

Console.WriteLine("Experiment Runner");

// We'll run RandomAlgorithm for a set of TAi instances and varying sample counts

// TAi base names to test
var taiBaseNames = new[]
{
                "tai_20_5_0",
                "tai_20_10_0",
                "tai_20_20_0",
                "tai_100_10_0",
                "tai_100_20_0",
                "tai_500_20_0"
            };

var sampleCounts = new[] { 1, 10, 100, 1000, 10000, 100000, 1000000 };
int seed = 123;

var algoSpecs = sampleCounts.Select(s => (Samples: s, Seed: seed));
var algorithms = AlgorithmFactory.CreateRandomAlgorithms(algoSpecs);

var problems = ProblemLoader.LoadMany(taiBaseNames);

var results = new List<object>();
// Determine ExperimentRunner project directory from the assembly output path (bin/...)
var baseDir = AppContext.BaseDirectory ?? Environment.CurrentDirectory;
// bin/{config}/{tfm} -> go up three levels to reach project directory
var projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
var outDir = Path.Combine(projectDir, "experiment_results");
Directory.CreateDirectory(outDir);

// write CSV header with Instance and Algorithm columns
var header = "Instance,Algorithm,Params,Seed,BestCost,Evaluations,ElapsedMs,BestFoundAt,Timestamp";
File.WriteAllText(Path.Combine(outDir, "random_sample_study.csv"), header + Environment.NewLine);

foreach (var (name, inst) in problems)
{
    foreach (var (algName, alg, pars) in algorithms)
    {
        var seedVal = pars is RandomParameters rpp ? rpp.Seed : (int?)null;
        Visualizer.DisplayRunStart(name, algName, seedVal);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = alg.Solve(inst, pars);
        sw.Stop();

        var record = new
        {
            Instance = name,
            Algorithm = algName,
            Params = pars.GetType().Name,
            Seed = pars is RandomParameters rp ? rp.Seed : (int?)null,
            BestCost = (result.Best as PermutationSolution)?.Cost,
            result.Evaluations,
            ElapsedMs = sw.Elapsed.TotalMilliseconds,
            BestFoundAt = result.BestFoundAtEvaluation,
            Timestamp = DateTimeOffset.UtcNow
        };
        results.Add(record);
        var line = $"{record.Instance},{record.Algorithm},{record.Params},{record.Seed},{record.BestCost},{record.Evaluations},{record.ElapsedMs},{record.BestFoundAt},{record.Timestamp:o}";
        ResultSaver.AppendCsvLine(outDir, "random_sample_study.csv", line);
        Visualizer.DisplayLine(line);
    }
}

// Save full JSON
ResultSaver.SaveJson(outDir, $"random_sample_study_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json", results);
Visualizer.DisplayLine($"Done. Results saved to {outDir}");
