using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using PFSP.Instances;
using PFSP.Solutions;
using PFSP.Solutions.PermutationSolutionGenerators;
using System.Buffers;

namespace PFSP.Algorithms.Greedy
{
    /// <summary>
    /// Implements the NEH (Nawaz-Enscore-Ham) heuristic for permutation flow shop scheduling.
    /// </summary>
    public class GreedyAlgorithm() : IAlgorithm
    {

        public AlgorithmResult Solve(Instance instance, IParameters parameters, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(instance);
            var p = parameters as GreedyParameters ?? throw new ArgumentException("parameters must be GreedyParameters", nameof(parameters));

            var sw = Stopwatch.StartNew();
            var sol = NehConstructive(instance);
            sw.Stop();

            return new AlgorithmResult(sol, Evaluations: 1, Elapsed: sw.Elapsed);
        }

        /// <summary>
        /// Constructs a permutation using the NEH (Nawaz-Enscore-Ham) heuristic.
        ///
        /// NEH is a classic constructive algorithm for permutation flow shop scheduling:
        /// 1. Compute a priority for each job (total processing time across machines) and sort jobs in
        ///    non-increasing order of priority.
        /// 2. Start with the highest priority job as the initial partial permutation.
        /// 3. Iteratively insert the next job from the sorted list into the position of the current partial
        ///    permutation that yields the best objective value (evaluated by the instance's evaluator).
        ///
        /// <param name="instance">The problem instance to construct a solution for.</param>
        /// <returns>A constructed <see cref="PermutationSolution"/>.</returns>
        private PermutationSolution NehConstructive(Instance instance)
        {
            ArgumentNullException.ThrowIfNull(instance);

            int jobs = instance.Jobs;
            int machines = instance.Machines;

            // Create a priority queue of jobs ordered by non-increasing total processing time.
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

            // Return a solution wrapping a fresh array copy of the permutation
            var result = partial.ToArray();
            var finalCost = instance.Evaluate(result);
            return PermutationSolution.WrapBuffer(result, finalCost);
        }
    }
}
