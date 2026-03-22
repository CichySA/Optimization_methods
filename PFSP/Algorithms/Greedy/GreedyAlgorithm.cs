using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using PFSP.Instances;
using PFSP.Solutions;

namespace PFSP.Algorithms.Greedy
{
    /// <summary>
    /// True greedy baseline without insertion search.
    /// Jobs are ordered once by ascending total processing time and returned as-is.
    /// </summary>
    public class GreedyAlgorithm() : IAlgorithm
    {
        public AlgorithmResult Solve(Instance instance, IParameters parameters, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(instance);
            var p = parameters as GreedyParameters ?? throw new ArgumentException("parameters must be GreedyParameters", nameof(parameters));

            var sw = Stopwatch.StartNew();
            var sol = ConstructNoSearch(instance);
            sw.Stop();

            return new AlgorithmResult(sol, Evaluations: 1, Elapsed: sw.Elapsed);
        }

        private static PermutationSolution ConstructNoSearch(Instance instance)
        {
            int jobs = instance.Jobs;
            int machines = instance.Machines;
            var order = new List<(int Job, double TotalTime)>(jobs);

            for (int j = 0; j < jobs; j++)
            {
                double total = 0.0;
                for (int m = 0; m < machines; m++)
                    total += instance.Matrix[m, j];
                order.Add((j, total));
            }

            // SPT tie-break by job index for deterministic behavior.
            order.Sort((a, b) =>
            {
                int cmp = a.TotalTime.CompareTo(b.TotalTime);
                return cmp != 0 ? cmp : a.Job.CompareTo(b.Job);
            });

            var permutation = new int[jobs];
            for (int i = 0; i < jobs; i++)
                permutation[i] = order[i].Job;

            double cost = instance.Evaluate(permutation);
            return PermutationSolution.WrapBuffer(permutation, cost);
        }
    }

    /// <summary>
    /// SPT-ordered NEH-like iterative greedy constructive heuristic.
    /// Starts from SPT priority and inserts each next job into the best position
    /// in the current partial sequence.
    /// </summary>
    public class SptAlgorithm() : IAlgorithm
    {
        public AlgorithmResult Solve(Instance instance, IParameters parameters, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(instance);
            var p = parameters as GreedyParameters ?? throw new ArgumentException("parameters must be GreedyParameters", nameof(parameters));

            var sw = Stopwatch.StartNew();
            var sol = SptIterativeInsertionConstructive(instance);
            sw.Stop();

            return new AlgorithmResult(sol, Evaluations: 1, Elapsed: sw.Elapsed);
        }

        private static PermutationSolution SptIterativeInsertionConstructive(Instance instance)
        {
            ArgumentNullException.ThrowIfNull(instance);

            int jobs = instance.Jobs;
            int machines = instance.Machines;

            // Priority queue ordered by ascending total processing time (SPT order).
            var jobPriorityQueue = new PriorityQueue<int, double>();
            for (int j = 0; j < jobs; j++)
            {
                double sum = 0.0;
                for (int m = 0; m < machines; m++)
                    sum += instance.Matrix[m, j];
                jobPriorityQueue.Enqueue(j, sum);
            }

            // Permutation formed by iterative insertion
            var partial = new List<int>(jobs);

            // Seed with first job if available
            if (jobPriorityQueue.Count > 0)
                partial.Add(jobPriorityQueue.Dequeue());

            while (jobPriorityQueue.Count > 0)
            {
                int job = jobPriorityQueue.Dequeue();

                int bestPos = 0;
                double bestCost = double.PositiveInfinity;

                for (int insertPos = 0; insertPos <= partial.Count; insertPos++)
                {
                    // Insert at position, evaluate, then remove to restore state
                    partial.Insert(insertPos, job);
                    var span = partial.ToArray(); // Create a temporary array for evaluation
                    double cost = instance.Evaluate(span);
                    partial.RemoveAt(insertPos);

                    if (cost < bestCost)
                    {
                        bestCost = cost;
                        bestPos = insertPos;
                    }
                }

                // Insert at best position
                partial.Insert(bestPos, job);
            }

            // Ensure a full permutation
            if (partial.Count != jobs)
                throw new InvalidOperationException($"Incomplete solution: constructed prefix length {partial.Count} does not equal number of jobs {jobs}.");

            // Return a solution wrapping a fresh array copy of the permutation.
            var result = partial.ToArray();
            var finalCost = instance.Evaluate(result);
            return PermutationSolution.WrapBuffer(result, finalCost);
        }
    }
}
