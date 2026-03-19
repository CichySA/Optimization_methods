using System.Globalization;
using ExperimentRunner;
using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.Random;
using PFSP.Solutions;

var config = ExperimentRunnerConfigurationCliParser.Parse(args);
if (config is null)
    return;

Console.WriteLine("Experiment Runner");

var algorithms = config.Algorithms.SelectMany(AlgorithmFactory.CreateManyFromSpec).ToList();
var problems = ProblemLoader.LoadMany(config.Instances);
var results = new List<object>();

var baseDir = AppContext.BaseDirectory ?? Environment.CurrentDirectory;
// Root the output directory at the solution level for universal accessibility
var solutionDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
// outDir is relative to solutionDir if not absolute
var outDir = Path.IsPathRooted(config.OutDir)
    ? config.OutDir
    : Path.Combine(solutionDir, config.OutDir);

Directory.CreateDirectory(outDir);

const string csvFileName = "experiment_study.csv";
var header = "Instance,Algorithm,Params,Seed,BestCost,Evaluations,ElapsedMs,BestFoundAt,Timestamp";
File.WriteAllText(Path.Combine(outDir, csvFileName), header + Environment.NewLine);

foreach (var (name, inst) in problems)
{
    foreach (var (algName, alg, pars) in algorithms)
    {
        var seedVal = pars switch
        {
            RandomParameters rp         => (int?)rp.Seed,
            EvolutionaryParameters ep   => (int?)ep.Seed,
            _                           => null
        };
        Visualizer.DisplayRunStart(name, algName, seedVal);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = alg.Solve(inst, pars);
        sw.Stop();

        var record = new
        {
            Instance = name,
            Algorithm = algName,
            Params = pars.GetType().Name,
            Seed = seedVal,
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
        ResultSaver.AppendCsvLine(outDir, csvFileName, line);
        Visualizer.DisplayLine(line);
    }
}

//ResultSaver.SaveJson(outDir, $"experiment_study_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json", results);
Visualizer.DisplayLine($"Done. Results saved to {outDir}");
