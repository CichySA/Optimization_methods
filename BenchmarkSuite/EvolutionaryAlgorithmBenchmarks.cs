using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;
using PFSP.Algorithms.Evolutionary;
using PFSP.Instances;

namespace BenchmarkSuite
{
    /// <summary>
    /// Benchmarks for EvolutionaryAlgorithm on tai100_10_0.
    ///
    /// Known inefficiencies under measurement:
    ///   1. Elitism sort  — OrderBy(cost).Take(k) does a full O(n log n) sort every generation.
    ///   2. nextPopulation alloc — new PermutationSolution[n] allocated every generation;
    ///                             could be eliminated with ping-pong buffers in state.
    ///   3. Child2 waste  — when the last slot is odd, crossover + mutation produce child2
    ///                      but it is discarded after the break.
    ///
    /// Run with and without elitism to isolate issue 1.
    /// MemoryDiagnoser quantifies issues 2 and 3 via allocation counts.
    /// </summary>
    [CPUUsageDiagnoser]
    [MemoryDiagnoser]
    public class EvolutionaryAlgorithmBenchmarks
    {
        private Instance _instance = null!;
        private EvolutionaryParameters _defaultParams = null!;
        private EvolutionaryParameters _elitismParams = null!;
        private EvolutionaryAlgorithm _algorithm = null!;

        [GlobalSetup]
        public void Setup()
        {
            _instance = InstanceReader.Read(100, 10, 0);
            _algorithm = new EvolutionaryAlgorithm();

            _defaultParams = new EvolutionaryParameters
            {
                Seed = 42,
                PopulationSize = 100,
                Generations = 200,
                ElitismK = 0,
            };

            _elitismParams = new EvolutionaryParameters
            {
                Seed = 42,
                PopulationSize = 100,
                Generations = 200,
                ElitismK = 5,
            };
        }

        [Benchmark(Baseline = true)]
        public void Solve_NoElitism()
        {
            _algorithm.Solve(_instance, _defaultParams);
        }

        [Benchmark]
        public void Solve_WithElitism()
        {
            _algorithm.Solve(_instance, _elitismParams);
        }
    }
}
