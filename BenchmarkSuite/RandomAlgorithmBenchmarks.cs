using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;
using PFSP.Algorithms.Random;
using PFSP.Evaluators;
using PFSP.Instances;
using System;

namespace BenchmarkSuite
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
            var matrix = new double[machines, jobs];
            var rnd = new Random(123);
            for (int m = 0; m < machines; m++)
                for (int j = 0; j < jobs; j++)
                    matrix[m, j] = rnd.Next(1, 100);

            _instance = Instance.Create(matrix, new TotalFlowTimeEvaluator());
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