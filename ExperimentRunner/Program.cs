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

const string jsonFileName = "experiment_results.json";

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

foreach (var record in records)
{
    Visualizer.DisplayLine($"{record.Instance} | {record.Algorithm} | Seed={record.Seed?.ToString() ?? "-"} | Best={record.Best?.Cost.ToString() ?? "-"} | Metrics={record.Metrics.Count}");
}

ResultSaver.SaveJson(outDir, jsonFileName, records);
Visualizer.DisplayLine($"Done. Results saved to {Path.Combine(outDir, jsonFileName)}");
