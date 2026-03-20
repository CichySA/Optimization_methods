using System.Globalization;
using ExperimentRunner;
using PFSP.Algorithms.Evolutionary;
using PFSP.Algorithms.RandomSearch;
using PFSP.Solutions;

var config = ExperimentRunnerConfigurationCliParser.Parse(args);
if (config is null)
    return;

Console.WriteLine("Experiment Runner");

var algorithms = config.Algorithms.SelectMany(AlgorithmFactory.CreateManyFromSpec).ToList();
var problems = ProblemLoader.LoadMany(config.Instances);
var outDir = ResultSaver.ResolveOutputDirectory(config.OutDir);

const string csvFileName = "experiment_study.csv";
var header = "Instance,Algorithm,Params,Seed,BestCost,Evaluations,ElapsedMs,BestFoundAt,Timestamp";

var runPlan = new List<(int Index, string InstanceName, PFSP.Instances.Instance Instance, string AlgorithmName, PFSP.Algorithms.IAlgorithm AlgorithmTemplate, PFSP.Algorithms.IParameters Parameters)>();
int orderedIndex = 0;
foreach (var (name, inst) in problems)
{
    foreach (var (algName, alg, pars) in algorithms)
    {
        runPlan.Add((orderedIndex++, name, inst, algName, alg, pars));
    }
}

var records = new object[runPlan.Count];
var csvLines = new string[runPlan.Count];

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
                _ => null
            };

            lock (outputLock)
                Visualizer.DisplayRunStart(run.InstanceName, run.AlgorithmName, seedVal);

            var algorithm = (PFSP.Algorithms.IAlgorithm)Activator.CreateInstance(run.AlgorithmTemplate.GetType())!;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = algorithm.Solve(run.Instance, run.Parameters);
            sw.Stop();

            var record = new
            {
                Instance = run.InstanceName,
                Algorithm = run.AlgorithmName,
                Params = run.Parameters.GetType().Name,
                Seed = seedVal,
                BestCost = (result.Best as PermutationSolution)?.Cost,
                result.Evaluations,
                ElapsedMs = sw.Elapsed.TotalMilliseconds,
                BestFoundAt = result.BestFoundAtEvaluation,
                Timestamp = DateTimeOffset.UtcNow
            };

            var bestCostStr = record.BestCost is double d
                ? d.ToString("G", CultureInfo.InvariantCulture)
                : record.BestCost?.ToString();

            var elapsedMsStr = record.ElapsedMs is double d2
                ? d2.ToString("G", CultureInfo.InvariantCulture)
                : record.ElapsedMs.ToString(CultureInfo.InvariantCulture);

            var line = $"{record.Instance},{record.Algorithm},{record.Params},{record.Seed},{bestCostStr},{record.Evaluations},{elapsedMsStr},{record.BestFoundAt},{record.Timestamp:o}";

            records[run.Index] = record;
            csvLines[run.Index] = line;
        }
    }))
    .ToArray();

await Task.WhenAll(workerTasks);

foreach (var line in csvLines)
    Visualizer.DisplayLine(line);

ResultSaver.SaveCsv(outDir, csvFileName, header, csvLines);

var results = records.ToList();
//ResultSaver.SaveJson(outDir, $"experiment_study_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json", results);
Visualizer.DisplayLine($"Done. Results saved to {outDir}");
