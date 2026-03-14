using System;
using BenchmarkDotNet.Attributes;
using PFSP.Algorithms.Random;
using PFSP.Instances;
using PFSP.Solutions;
using PFSP.Solutions.PermutationSolutionGenerators;
using Microsoft.VSDiagnostics;

namespace PFSP.Benchmarks
{
    [CPUUsageDiagnoser]
    public class RandomAlgorithmBenchmarks
    {
        private Instance _instance;
        private RandomParameters _parameters;
        private RandomAlgorithm _algorithm;
        [GlobalSetup]
        public void Setup()
        {
            int jobs = 100; // representative size; adjust if needed
            int machines = 10;
            _instance = new Instance
            {
                Jobs = jobs,
                Machines = machines,
                Matrix = new double[machines, jobs],
                Evaluator = new PFSP.Evaluators.TotalFlowTimeEvaluator()
            };
            var rnd = new Random(123);
            for (int m = 0; m < machines; m++)
                for (int j = 0; j < jobs; j++)
                    _instance.Matrix[m, j] = rnd.Next(1, 100);
            _parameters = RandomParameters.ForRuns(1000, seed: 123);
            _algorithm = new RandomAlgorithm();
        }

        [Benchmark]
        public void SolveRandomAlgorithm()
        {
            _algorithm.Solve(_instance, _parameters);
        }
    }
}