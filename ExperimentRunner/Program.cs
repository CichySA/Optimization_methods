using System.Globalization;
using ExperimentRunner;
using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.RandomSearch;

var config = ExperimentRunnerConfigurationCliParser.Parse(args);
if (config is null)
    return;

Console.WriteLine("Experiment Runner");

var algorithms = config.Algorithms
    .SelectMany(spec => AlgorithmFactory.CreateFromSpec(spec, config.Parameters))
    .ToList();
var problems = ProblemLoader.LoadMany(config.Instances);
var outDir = ResultSaver.ResolveOutputDirectory(config.OutDir);
var experimentName = PathResolver.ToSafeFileName(config.ExperimentName);
var resultJsonFileName = $"result_{experimentName}.json";
var configJsonFileName = $"config_{experimentName}.json";
var configOutputPath = PathResolver.ResolveOutputFilePath(outDir, configJsonFileName);

if (!string.IsNullOrWhiteSpace(config.ConfigPath) && File.Exists(config.ConfigPath))
{
    var sourcePath = Path.GetFullPath(config.ConfigPath);
    var destinationPath = Path.GetFullPath(configOutputPath);

    if (string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase))
    {
        Visualizer.DisplayLine($"Config already in output directory: {destinationPath}");
    }
    else if (File.Exists(destinationPath))
    {
        Visualizer.DisplayLine($"Config file already exists, leaving unchanged: {destinationPath}");
    }
    else
    {
        File.Copy(sourcePath, destinationPath, overwrite: false);
    }
}
else
{
    if (!File.Exists(configOutputPath))
    {
        ResultSaver.SaveJson(outDir, configJsonFileName, config);
    }
}

var runPlan = new List<(int Index, string InstanceName, PFSP.Instances.Instance Instance, string AlgorithmName, PFSP.Algorithms.IAlgorithm AlgorithmTemplate, PFSP.Algorithms.IParameters Parameters)>();
int orderedIndex = 0;
foreach (var (name, inst) in problems)
{
    foreach (var (algName, alg, pars) in algorithms)
    {
        runPlan.Add((orderedIndex++, name, inst, algName, alg, pars));
    }
}

var records = new ExperimentRunRecord[runPlan.Count];

var shuffled = Enumerable.Range(0, runPlan.Count).ToArray();
for (int i = shuffled.Length - 1; i > 0; i--)
{
    var j = Random.Shared.Next(i + 1);
    (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
}

var workerCount = Math.Max(1, Math.Min(Environment.ProcessorCount, runPlan.Count));
var workerBuckets = Enumerable.Range(0, workerCount).Select(_ => new List<int>()).ToArray();
for (int i = 0; i < shuffled.Length; i++)
    workerBuckets[i % workerCount].Add(shuffled[i]);

var outputLock = new object();

var workerTasks = workerBuckets
    .Where(bucket => bucket.Count > 0)
    .Select(bucket => Task.Run(() =>
    {
        foreach (var plannedRunIndex in bucket)
        {
            var run = runPlan[plannedRunIndex];
            var seedVal = run.Parameters switch
            {
                RandomSearchParameters rp => (int?)rp.Seed,
                EvolutionaryParameters ep => (int?)ep.Seed,
                PFSP.Algorithms.SimulatedAnnealing.SimulatedAnnealingParameters sp => (int?)sp.Seed,
                _ => null
            };

            lock (outputLock)
                Visualizer.DisplayRunStart(run.InstanceName, run.AlgorithmName, seedVal);

            var algorithm = (PFSP.Algorithms.IAlgorithm)Activator.CreateInstance(run.AlgorithmTemplate.GetType())!;
            var result = algorithm.Solve(run.Instance, run.Parameters);
            var record = ExperimentRunRecordFactory.Create(
                run.InstanceName,
                run.AlgorithmName,
                run.Parameters,
                seedVal,
                result,
                DateTimeOffset.UtcNow);

            records[run.Index] = record;
        }
    }))
    .ToArray();

await Task.WhenAll(workerTasks);

ResultSaver.SaveJson(outDir, resultJsonFileName, records);
Visualizer.DisplayLine($"Done. Results saved to {Path.Combine(outDir, resultJsonFileName)}");
