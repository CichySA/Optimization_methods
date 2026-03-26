using PFSP.Monitoring;
using PFSP.Instances;
using PFSP.Solutions;

namespace PFSP.Algorithms.Greedy
{
    public class GreedyAlgorithm() : IAlgorithm
    {
        public AlgorithmResult Solve(Instance instance, IParameters parameters, CancellationToken cancellationToken = default) =>
            GreedyAlgorithmCore.SolveConstructive(
                instance,
                parameters,
                static currentInstance =>
                {
                    var permutation = Enumerable.Range(0, currentInstance.Jobs)
                        .Select(job => (Job: job, TotalTime: GreedyAlgorithmCore.TotalProcessingTime(currentInstance, job)))
                        .OrderBy(item => item.TotalTime)
                        .ThenBy(item => item.Job)
                        .Select(item => item.Job)
                        .ToArray();

                    return PermutationSolution.WrapBuffer(permutation, currentInstance.Evaluate(permutation));
                },
                emitStepCompleted: false);
    }

    public class SptAlgorithm() : IAlgorithm
    {
        public AlgorithmResult Solve(Instance instance, IParameters parameters, CancellationToken cancellationToken = default) =>
            GreedyAlgorithmCore.SolveConstructive(instance, parameters, GreedyAlgorithmCore.SptIterativeInsertionConstructive, emitStepCompleted: true);
    }

    internal static class GreedyAlgorithmCore
    {
        internal static AlgorithmResult SolveConstructive(
            Instance instance,
            IParameters parameters,
            Func<Instance, PermutationSolution> construct,
            bool emitStepCompleted)
        {
            ArgumentNullException.ThrowIfNull(instance);
            ArgumentNullException.ThrowIfNull(construct);
            var greedyParameters = parameters as GreedyParameters ?? throw new ArgumentException("parameters must be GreedyParameters", nameof(parameters));

            var result = new AlgorithmResult(parameters);
            var state = new GreedyAlgorithmState(instance, greedyParameters);
            var monitor = new AlgorithmMonitor(result, greedyParameters.Monitoring);

            monitor.Emit(AlgorithmEventKind.Started, state);

            state.Candidate = construct(instance);
            state.Best = state.Candidate;
            if (emitStepCompleted)
                state.Step = 1;

            result.SetBest(state.Candidate);

            if (emitStepCompleted)
                monitor.Emit(AlgorithmEventKind.StepCompleted, state);

            monitor.Emit(AlgorithmEventKind.Finished, state);
            return result;
        }

        internal static PermutationSolution SptIterativeInsertionConstructive(Instance instance)
        {
            ArgumentNullException.ThrowIfNull(instance);

            int jobs = instance.Jobs;
            var jobPriorityQueue = new PriorityQueue<int, double>();
            for (int j = 0; j < jobs; j++)
                jobPriorityQueue.Enqueue(j, TotalProcessingTime(instance, j));

            var partial = new List<int>(jobs);
            if (jobPriorityQueue.Count > 0)
                partial.Add(jobPriorityQueue.Dequeue());

            while (jobPriorityQueue.Count > 0)
            {
                int job = jobPriorityQueue.Dequeue();
                int bestPos = 0;
                double bestCost = double.PositiveInfinity;

                for (int insertPos = 0; insertPos <= partial.Count; insertPos++)
                {
                    partial.Insert(insertPos, job);
                    var span = partial.ToArray();
                    double cost = instance.Evaluate(span);
                    partial.RemoveAt(insertPos);

                    if (cost < bestCost)
                    {
                        bestCost = cost;
                        bestPos = insertPos;
                    }
                }

                partial.Insert(bestPos, job);
            }

            if (partial.Count != jobs)
                throw new InvalidOperationException($"Incomplete solution: constructed prefix length {partial.Count} does not equal number of jobs {jobs}.");

            var permutation = partial.ToArray();
            return PermutationSolution.WrapBuffer(permutation, instance.Evaluate(permutation));
        }

        internal static double TotalProcessingTime(Instance instance, int job)
        {
            double total = 0.0;
            for (int machine = 0; machine < instance.Machines; machine++)
                total += instance.Matrix[machine, job];
            return total;
        }
    }
}
