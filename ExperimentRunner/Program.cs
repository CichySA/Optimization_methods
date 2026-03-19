using System.Globalization;
using ExperimentRunner;
using PFSP.Algorithms.Random;
using PFSP.Solutions;

var config = ExperimentRunnerConfigurationCliParser.Parse(args);
if (config is null)
    return;

Console.WriteLine("Experiment Runner");

var algoSpecs = config.Samples.Select(s => (Samples: s, Seed: config.Seed));
var algorithms = AlgorithmFactory.CreateRandomAlgorithms(algoSpecs);
var problems = ProblemLoader.LoadMany(config.Instances);
var results = new List<object>();

var baseDir = AppContext.BaseDirectory ?? Environment.CurrentDirectory;
var projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
var outDir = Path.IsPathRooted(config.OutDir)
    ? config.OutDir
    : Path.Combine(projectDir, config.OutDir);

Directory.CreateDirectory(outDir);

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

        var bestCostStr = record.BestCost is double d
            ? d.ToString("G", CultureInfo.InvariantCulture)
            : record.BestCost?.ToString();

        var elapsedMsStr = record.ElapsedMs is double d2
            ? d2.ToString("G", CultureInfo.InvariantCulture)
            : record.ElapsedMs.ToString();

        var line = $"{record.Instance},{record.Algorithm},{record.Params},{record.Seed},{bestCostStr},{record.Evaluations},{elapsedMsStr},{record.BestFoundAt},{record.Timestamp:o}";
        ResultSaver.AppendCsvLine(outDir, "random_sample_study.csv", line);
        Visualizer.DisplayLine(line);
    }
}

ResultSaver.SaveJson(outDir, $"random_sample_study_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json", results);
Visualizer.DisplayLine($"Done. Results saved to {outDir}");
